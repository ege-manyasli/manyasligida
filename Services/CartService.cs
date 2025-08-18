using Microsoft.AspNetCore.Http;
using System.Text.Json;
using manyasligida.Models;
using System.Threading;

namespace manyasligida.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CartSessionKey = "Cart";
        private static readonly SemaphoreSlim _cartLock = new SemaphoreSlim(1, 1);

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<CartItem>> GetCartAsync()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
                return new List<CartItem>();
            
            // Ensure session ID exists
            var sessionId = session.GetString("SessionId");
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                session.SetString("SessionId", sessionId);
            }
                
            var cartJson = session.GetString(CartSessionKey);
            
            if (string.IsNullOrEmpty(cartJson))
                return new List<CartItem>();

            try
            {
                return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
            }
            catch
            {
                return new List<CartItem>();
            }
        }

        public List<CartItem> GetCart()
        {
            return GetCartAsync().GetAwaiter().GetResult();
        }

        public async Task SaveCartAsync(List<CartItem> cart)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
                return;
                
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
            
            var cartJson = JsonSerializer.Serialize(cart, options);
            session.SetString(CartSessionKey, cartJson);
        }

        public void SaveCart(List<CartItem> cart)
        {
            SaveCartAsync(cart).GetAwaiter().GetResult();
        }

        public async Task AddToCartAsync(Product product, int quantity = 1)
        {
            await _cartLock.WaitAsync();
            try
            {
                var cart = await GetCartAsync();
                var existingItem = cart.FirstOrDefault(item => item.ProductId == product.Id);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.Id,
                        Quantity = quantity,
                        UnitPrice = product.Price,
                        TotalPrice = product.Price * quantity,
                        AddedAt = DateTime.Now
                    });
                }

                await SaveCartAsync(cart);
            }
            finally
            {
                _cartLock.Release();
            }
        }

        public void AddToCart(Product product, int quantity = 1)
        {
            AddToCartAsync(product, quantity).GetAwaiter().GetResult();
        }

        public async Task UpdateQuantityAsync(int productId, int quantity)
        {
            await _cartLock.WaitAsync();
            try
            {
                var cart = await GetCartAsync();
                var item = cart.FirstOrDefault(i => i.ProductId == productId);

                if (item != null)
                {
                    if (quantity <= 0)
                    {
                        cart.Remove(item);
                    }
                    else
                    {
                        item.Quantity = quantity;
                        item.TotalPrice = item.Quantity * item.UnitPrice;
                    }
                    await SaveCartAsync(cart);
                }
            }
            finally
            {
                _cartLock.Release();
            }
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            UpdateQuantityAsync(productId, quantity).GetAwaiter().GetResult();
        }

        public async Task RemoveFromCartAsync(int productId)
        {
            await _cartLock.WaitAsync();
            try
            {
                var cart = await GetCartAsync();
                var item = cart.FirstOrDefault(i => i.ProductId == productId);
                
                if (item != null)
                {
                    cart.Remove(item);
                    await SaveCartAsync(cart);
                }
            }
            finally
            {
                _cartLock.Release();
            }
        }

        public void RemoveFromCart(int productId)
        {
            RemoveFromCartAsync(productId).GetAwaiter().GetResult();
        }

        public void ClearCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
                return;
                
            session.Remove(CartSessionKey);
        }

        public async Task<int> GetCartItemCountAsync()
        {
            var cart = await GetCartAsync();
            return cart.Sum(item => item.Quantity);
        }

        public int GetCartItemCount()
        {
            return GetCartItemCountAsync().GetAwaiter().GetResult();
        }

        public async Task<decimal> GetCartTotalAsync()
        {
            var cart = await GetCartAsync();
            return cart.Sum(item => item.TotalPrice);
        }

        public decimal GetCartTotal()
        {
            return GetCartTotalAsync().GetAwaiter().GetResult();
        }
    }
} 
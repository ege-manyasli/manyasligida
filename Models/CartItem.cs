namespace manyasligida.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int? CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;
        
        // Navigation properties (only for database operations)
        public Cart? Cart { get; set; }
        public Product? Product { get; set; }
    }
} 
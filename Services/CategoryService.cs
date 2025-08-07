using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using manyasligida.Data;
using manyasligida.Models;

namespace manyasligida.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string CATEGORIES_CACHE_KEY = "active_categories";
        private const int CACHE_EXPIRATION_MINUTES = 30;

        public CategoryService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<Category>> GetActiveCategoriesAsync()
        {
            return await _cache.GetOrCreateAsync(CATEGORIES_CACHE_KEY, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES);
                
                return await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();
            }) ?? new List<Category>();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            var categories = await GetActiveCategoriesAsync();
            return categories.FirstOrDefault(c => c.Id == id);
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            var categories = await GetActiveCategoriesAsync();
            return categories.Any(c => c.Id == id);
        }

        public void ClearCache()
        {
            _cache.Remove(CATEGORIES_CACHE_KEY);
        }
    }
}
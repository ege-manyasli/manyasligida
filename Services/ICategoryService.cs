using manyasligida.Models;

namespace manyasligida.Services
{
    public interface ICategoryService
    {
        Task<List<Category>> GetActiveCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<bool> CategoryExistsAsync(int id);
        void ClearCache();
    }
}
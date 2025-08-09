using manyasligida.Models.DTOs;

namespace manyasligida.Services.Interfaces
{
    public interface IAboutService
    {
        Task<AboutServiceResponse<AboutContentResponse>> GetAboutContentAsync();
        Task<AboutServiceResponse<AboutContentResponse>> UpdateAboutContentAsync(AboutEditRequest request);
        Task<AboutServiceResponse<AboutContentResponse>> CreateDefaultAboutContentAsync();
        Task<AboutServiceResponse<bool>> DeleteAboutContentAsync(int id);
        Task<AboutServiceResponse<List<AboutContentResponse>>> GetAllAboutContentsAsync();
    }
}

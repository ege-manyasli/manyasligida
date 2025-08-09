using manyasligida.Models.DTOs;

namespace manyasligida.Services.Interfaces
{
    public interface IHomeService
    {
        Task<HomeServiceResponse<HomeContentResponse>> GetHomeContentAsync();
        Task<HomeServiceResponse<HomeContentResponse>> UpdateHomeContentAsync(HomeEditRequest request);
        Task<HomeServiceResponse<HomeContentResponse>> CreateDefaultHomeContentAsync();
        Task<HomeServiceResponse<bool>> DeleteHomeContentAsync(int id);
        Task<HomeServiceResponse<List<HomeContentResponse>>> GetAllHomeContentsAsync();
    }
}

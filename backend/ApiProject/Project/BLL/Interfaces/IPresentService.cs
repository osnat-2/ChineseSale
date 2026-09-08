using Project.Dto;
using Project.Models;

namespace Project.Bll.Interfaces
{
    public interface IPresentService
    {
        Task<Result<Present>> GetAllPresentsAsync(bool onlyActive = true);
        Task<Result<Present>> GetPresentByIdAsync(int id);
        Task<Result<Present>> AddPresentAsync(PresentDto presentDto);
        Task<Result<Present>> DeletePresentAsync(int presentId);
        Task<Result<Present>> UpdatePresentAsync(int id, PresentDto presentDto);
        Task<Result<Present>> GetPresentsByNameAsync(string name);
        Task<Result<Present>> GetPresentsByDonorNameAsync(string donorName);
        Task<Result<Present>> GetPresentsByBuyerCountAsync(int buyerCount);
        Task<Result<Present>> GetPresentsByPriceAsync();
        Task<Result<Present>> GetPresentsByCategoryAsync();
        Task<Result<Present>> GetAllPresentsWithWinnersAsync();
    }
}

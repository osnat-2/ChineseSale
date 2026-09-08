using Project.Models;

namespace Project.Dal.Interfaces
{
    public interface IPresentDal
    {
        Task<Result<Present>> GetAllPresentsAsync(bool onlyActive = true);
        Task<Result<Present>> GetPresentByIdAsync(int id);
        Task<Result<Present>> AddPresentAsync(Present present);
        Task<Result<Present>> DeletePresentAsync(int presentId);
        Task<Result<Present>> UpdatePresentAsync(Present present);
        Task<Result<Present>> GetPresentsByNameAsync(string name);
        Task<Result<Present>> GetPresentsByDonorNameAsync(string donorName);
        Task<Result<Present>> GetPresentsByBuyerCountAsync(int buyerCount);
        Task<Result<Present>> GetPresentsByPriceAsync();
        Task<Result<Present>> GetPresentsByCategoryAsync();
        Task<Result<Present>> GetAllPresentsWithWinnersAsync();
        Task<Result<Present>> GetAllPresentsNoIncludeAsync();
        Task<Result<bool>> PresentNameExistsAsync(string name, int? excludePresentId = null);
    }
}

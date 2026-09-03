using Project.Models;

namespace Project.DAL.Interfaces
{
    public interface IDonorDal
    {
        Task<Result<Donor>> GetAllDonorsAsync(bool onlyActive = true);
        Task<Result<Donor>> GetDonorByIdAsync(int id);
        Task<Result<Donor>> AddDonorAsync(Donor donor);
        Task<Result<Donor>> UpdateDonorAsync(Donor donor);
        Task<Result<Donor>> SoftDeleteDonorAsync(int id);
        Task<Result<Donor>> GetDonorsByEmailAsync(string email);
        Task<Result<Donor>> GetDonorsByNameAsync(string name);
        Task<Result<Donor>> GetDonorsByPresentNameAsync(string presentName);
        Task<bool> ExistingEmailAsync(string email, int? excludeId = null);
        Task<bool> DonorHasPresentsAsync(int donorId);
    }
}

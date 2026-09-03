using Project.Models;
using Project.Models.ModelsDTO;

namespace Project.BLL.Interfaces
{
    public interface IDonorService
    {
        Task<Result<Donor>> GetAllDonorsAsync(bool onlyActive = true);
        Task<Result<Donor>> GetDonorByIdAsync(int id);
        Task<Result<Donor>> AddDonorAsync(DonorDto donorDto, int createdById);
        Task<Result<Donor>> UpdateDonorAsync(int id, DonorDto donorDto, int updatedById);
        Task<Result<Donor>> DeleteDonorAsync(int id);
        Task<Result<Donor>> GetDonorsByEmailAsync(string email);
        Task<Result<Donor>> GetDonorsByNameAsync(string name);
        Task<Result<Donor>> GetDonorsByPresentNameAsync(string presentName);
    }
}
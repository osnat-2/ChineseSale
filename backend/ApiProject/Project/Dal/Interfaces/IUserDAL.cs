using Project.Models;

namespace Project.Dal.Interfaces
{
    public interface IUserDal
    {
        Task<Result<User>> Register(User user);
        Task<User?> GetUserByEmail(string email);
        Task<Role?> GetRoleByName(string name);
        Task<Result<User>> GetDonorsAsync();
        Task<Result<User>> UpdateDonorAsync(int id, User donorDetails);
        Task<Result<User>> DeleteDonorAsync(int id);
    }
}

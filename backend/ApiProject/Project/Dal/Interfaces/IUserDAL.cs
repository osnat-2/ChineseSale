using Project.Models;

namespace Project.Dal.Interfaces
{
    public interface IUserDal
    {
        Task<Result<User>> Register(User user);
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserById(int id);
        Task<Role?> GetRoleByName(string name);
        Task<Role?> GetRoleById(int id);
        Task<Result<User>> GetDonorsAsync();
        Task<Result<User>> UpdateDonorAsync(int id, User donorDetails);
        Task<Result<User>> DeleteDonorAsync(int id);
        Task<Result<User>> GetAllManagedUsersAsync();
        Task<Result<User>> CreateManagedUserAsync(User user);
        Task<Result<User>> UpdateManagedUserAsync(int id, User userDetails);
        Task<Result<User>> DeleteManagedUserAsync(int id);
    }
}

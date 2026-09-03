using Project.Models;

namespace Project.DAL.Interfaces
{
    public interface IUserDal
    {
        Task<Result<User>> Register(User user);
        Task<User> GetUserByEmail(string email);
    }
}

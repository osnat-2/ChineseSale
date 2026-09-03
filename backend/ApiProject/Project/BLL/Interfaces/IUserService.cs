using Project.Models;
using Project.Models.ModelsDTO;

namespace Project.BLL.Interfaces
{
    public interface IUserService
    {
        Task<Result<string>> Login(string email, string password);
        Task<Result<User>> Register(UserDto userDto);
    }
}

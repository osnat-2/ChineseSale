using Project.Dto;
using Project.Models;

namespace Project.Bll.Interfaces
{
    public interface IUserService
    {
        Task<Result<string>> Login(string email, string password);
        Task<Result<User>> Register(UserDto userDto);
        Task<Result<User>> AddDonor(UserDto userDto);
        Task<Result<User>> AddAdmin(UserDto userDto);
    }
}

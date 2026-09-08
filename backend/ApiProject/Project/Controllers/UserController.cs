using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Bll;
using Project.Bll.Interfaces;
using Project.Dal.Interfaces;
using Project.Dto;
using Project.Models;

namespace Project.Controllers
{
    [ApiController]
    [Route("api/auth")] // ������ �� ����� ������ ���
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // ����� �-POST �� ���� "login" ����� ����� �� ������� �-Body
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<Result<string>> LoginUserAsync([FromQuery] string email, [FromQuery] string password)
        {
            return await _userService.Login(email, password);
        }

        // ����� �-POST �� ���� "register" ����� ����� �� ������� �-Body
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<Result<User>> Register([FromBody] UserDto userDto) // ��� �� ������ �-FromBody
        {
            return await _userService.Register(userDto);
        }

        [HttpPost("addDonor")]
        [Authorize(Roles = "Admin")]
        public async Task<Result<User>> AddDonor([FromBody] UserDto userDto) //  -FromBody
        {
            return await _userService.AddDonor(userDto);
        }

        [HttpPost("addAdmin")]
        [Authorize(Roles = "Admin")]
        public async Task<Result<User>> AddAdmin([FromBody] UserDto userDto) //  -FromBody
        {
            return await _userService.AddAdmin(userDto);
        }
    }
}
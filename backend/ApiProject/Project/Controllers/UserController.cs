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

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginUserAsync([FromBody] LoginRequestDto loginRequest)
        {
            var result = await _userService.Login(loginRequest.Email, loginRequest.Password);
            if (result.Success)
            {
                return Ok(result);
            }
            return Unauthorized();
        }

        // ����� �-POST �� ���� "register" ����� ����� �� ������� �-Body
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserDto userDto) // ��� �� ������ �-FromBody
        {
            var result = await _userService.Register(userDto);
            var safeResult = ToSafeUserResult(result);
            return result.Success ? Ok(safeResult) : BadRequest(safeResult);
        }

        [HttpPost("addDonor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddDonor([FromBody] UserDto userDto) //  -FromBody
        {
            var result = await _userService.AddDonor(userDto);
            var safeResult = ToSafeUserResult(result);
            return result.Success ? Ok(safeResult) : BadRequest(safeResult);
        }

        [HttpPost("addAdmin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddAdmin([FromBody] UserDto userDto) //  -FromBody
        {
            var result = await _userService.AddAdmin(userDto);
            var safeResult = ToSafeUserResult(result);
            return result.Success ? Ok(safeResult) : BadRequest(safeResult);
        }

        private static Result<UserResponseDto> ToSafeUserResult(Result<User> result)
        {
            return new Result<UserResponseDto>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Data?.Select(user => new UserResponseDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Phone = user.Phone,
                    Email = user.Email,
                    IsActive = user.IsActive
                }).ToList()
            };
        }
    }
}
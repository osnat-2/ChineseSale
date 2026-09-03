using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.BLL;
using Project.BLL.Interfaces;
using Project.DAL.Interfaces;
using Project.Models;
using Project.Models.ModelsDTO;

namespace Project.Controllers
{
    [ApiController]
    [Route("api/auth")] // הוספתי את הנתיב הבסיסי כאן
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // משתמש ב-POST על נתיב "login" ומצפה לשלוח את הנתונים ב-Body
        [HttpPost("login")]
        public async Task<Result<string>> LoginUserAsync([FromQuery] string email, [FromQuery] string password)
        {
            return await _userService.Login(email, password);
        }

        // משתמש ב-POST על נתיב "register" ומצפה לשלוח את הנתונים ב-Body
        [HttpPost("register")]
        public async Task<Result<User>> Register([FromBody] UserDto userDto) // שים לב לשימוש ב-FromBody
        {
            return await _userService.Register(userDto);
        }
    }
}
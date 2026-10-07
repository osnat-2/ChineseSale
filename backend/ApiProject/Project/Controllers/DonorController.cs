using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Bll.Interfaces;
using Project.Dto;
using Project.Models;

namespace Project.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/donor")]
    public class DonorController : ControllerBase
    {
        private readonly IUserService _userService;

        public DonorController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<Result<UserResponseDto>>> GetDonors()
        {
            var result = await _userService.GetDonorsAsync();
            return Ok(ToSafeUserResult(result));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Result<UserResponseDto>>> UpdateDonor(
            int id,
            [FromBody] DonorUpdateDto donorDto)
        {
            if (id <= 0)
            {
                return BadRequest(new Result<UserResponseDto>
                {
                    Success = false,
                    Message = "Invalid donor id.",
                    Data = null
                });
            }

            var result = await _userService.UpdateDonorAsync(id, donorDto);
            var safeResult = ToSafeUserResult(result);
            if (result.Success)
            {
                return Ok(safeResult);
            }

            return result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(safeResult)
                : BadRequest(safeResult);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Result<UserResponseDto>>> DeleteDonor(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new Result<UserResponseDto>
                {
                    Success = false,
                    Message = "Invalid donor id.",
                    Data = null
                });
            }

            var result = await _userService.DeleteDonorAsync(id);
            var safeResult = ToSafeUserResult(result);
            if (result.Success)
            {
                return Ok(safeResult);
            }

            return result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(safeResult)
                : BadRequest(safeResult);
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
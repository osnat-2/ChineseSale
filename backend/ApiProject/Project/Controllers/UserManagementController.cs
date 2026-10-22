using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Bll.Interfaces;
using Project.Dto;
using Project.Models;

namespace Project.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public sealed class UserManagementController : ControllerBase
{
    private readonly IUserService _userService;

    public UserManagementController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<Result<ManagedUserResponseDto>>> GetUsers()
    {
        var result = await _userService.GetAllManagedUsersAsync();
        return Ok(ToSafeResult(result));
    }

    [HttpPost]
    public async Task<ActionResult<Result<ManagedUserResponseDto>>> CreateUser(
        [FromBody] ManagedUserCreateDto? userDto)
    {
        if (userDto == null)
        {
            return BadRequest(new Result<ManagedUserResponseDto>
            {
                Success = false,
                Message = "User details are required.",
                Data = null
            });
        }

        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var createdBy) ||
            createdBy <= 0)
        {
            return Unauthorized();
        }

        var result = await _userService.CreateManagedUserAsync(userDto, createdBy);
        if (!result.Success)
        {
            return BadRequest(ToSafeResult(result));
        }

        return Ok(ToSafeResult(result));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Result<ManagedUserResponseDto>>> UpdateUser(
        int id,
        [FromBody] ManagedUserUpdateDto? userDto)
    {
        if (id <= 0)
        {
            return BadRequest(new Result<ManagedUserResponseDto>
            {
                Success = false,
                Message = "Invalid user id.",
                Data = null
            });
        }

        if (userDto == null)
        {
            return BadRequest(new Result<ManagedUserResponseDto>
            {
                Success = false,
                Message = "User details are required.",
                Data = null
            });
        }

        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId) ||
            currentUserId <= 0)
        {
            return Unauthorized();
        }

        if (!userDto.IsActive && id == currentUserId)
        {
            return BadRequest(new Result<ManagedUserResponseDto>
            {
                Success = false,
                Message = "You cannot deactivate your own account.",
                Data = null
            });
        }

        var result = await _userService.UpdateManagedUserAsync(id, userDto);
        if (result.Success)
        {
            return Ok(ToSafeResult(result));
        }

        return result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
            ? NotFound(ToSafeResult(result))
            : BadRequest(ToSafeResult(result));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Result<ManagedUserResponseDto>>> DeleteUser(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new Result<ManagedUserResponseDto>
            {
                Success = false,
                Message = "Invalid user id.",
                Data = null
            });
        }

        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId) ||
            currentUserId <= 0)
        {
            return Unauthorized();
        }

        if (id == currentUserId)
        {
            return BadRequest(new Result<ManagedUserResponseDto>
            {
                Success = false,
                Message = "You cannot delete your own account.",
                Data = null
            });
        }

        var result = await _userService.DeleteManagedUserAsync(id);
        if (result.Success)
        {
            return Ok(ToSafeResult(result));
        }

        return result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
            ? NotFound(ToSafeResult(result))
            : BadRequest(ToSafeResult(result));
    }

    private static Result<ManagedUserResponseDto> ToSafeResult(Result<User> result) =>
        new()
        {
            Success = result.Success,
            Message = result.Message,
            Data = result.Data?.Select(user => new ManagedUserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Phone = user.Phone,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name ?? string.Empty,
                IsActive = user.IsActive
            }).ToList()
        };
}

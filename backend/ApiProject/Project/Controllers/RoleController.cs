using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Bll.Interfaces;
using Project.Dto;
using Project.Models;

namespace Project.Controllers
{
    [ApiController]
    [Route("api/role")]
    [Authorize(Roles = "Admin")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }
        [HttpGet]
        public async Task<ActionResult<Result<Role>>> GetAllRoles()
        {
            var result = await _roleService.GetAllRolesAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Result<Role>>> GetRoleById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new Result<Role>
                {
                    Success = false,
                    Message = "Invalid role ID."
                });
            }

            var result = await _roleService.GetRoleByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        public async Task<ActionResult<Result<Role>>> CreateRole([FromBody] RoleDto roleDto)
        {
            if (roleDto == null)
            {
                return BadRequest(new Result<Role>
                {
                    Success = false,
                    Message = "Role data is required."
                });
            }

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var createdBy) ||
                createdBy <= 0)
            {
                return Unauthorized();
            }

            var result = await _roleService.CreateRoleAsync(roleDto, createdBy);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            var createdRole = result.Data?.FirstOrDefault();
            return CreatedAtAction(
                nameof(GetRoleById),
                new { id = createdRole?.Id },
                result);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Result<Role>>> UpdateRole(int id, [FromBody] RoleDto roleDto)
        {
            if (id <= 0 || roleDto == null)
            {
                return BadRequest(new Result<Role>
                {
                    Success = false,
                    Message = "A valid role ID and role data are required."
                });
            }

            var result = await _roleService.UpdateRoleAsync(id, roleDto);
            if (result.Success)
            {
                return Ok(result);
            }

            var existingRole = await _roleService.GetRoleByIdAsync(id);
            return existingRole.Success ? BadRequest(result) : NotFound(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Result<Role>>> DeleteRole(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new Result<Role>
                {
                    Success = false,
                    Message = "Invalid role ID."
                });
            }

            var result = await _roleService.DeleteRoleAsync(id);
            if (result.Success)
            {
                return Ok(result);
            }

            var existingRole = await _roleService.GetRoleByIdAsync(id);
            return existingRole.Success ? BadRequest(result) : NotFound(result);
        }
    }
}

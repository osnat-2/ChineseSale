using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Bll.Interfaces;
using Project.Controllers;
using Project.Dto;
using Project.Models;
using Xunit;

namespace Project.Tests;

public class RoleControllerTests
{
    [Fact]
    public void RoleController_RequiresAdminForAllEndpoints()
    {
        var authorize = typeof(RoleController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        Assert.Equal("Admin", authorize.Roles);
    }

    [Fact]
    public async Task CreateRole_RequiresAuthenticatedUserIdForAudit()
    {
        var controller = new RoleController(new FakeRoleService())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        var result = await controller.CreateRole(
            new RoleDto { Name = "Auditor", Description = "Audit access" });

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task CreateRole_WithAuthenticatedUserId_ReturnsCreatedResponse()
    {
        var controller = new RoleController(new FakeRoleService())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, "17")
                    ]))
                }
            }
        };

        var result = await controller.CreateRole(
            new RoleDto { Name = "Auditor", Description = "Audit access" });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(RoleController.GetRoleById), created.ActionName);
    }

    [Fact]
    public async Task UpdateAndDeleteRole_WhenRoleDoesNotExist_ReturnNotFound()
    {
        var controller = new RoleController(new FakeRoleService(roleExists: false));

        var updated = await controller.UpdateRole(
            99,
            new RoleDto { Name = "Auditor", Description = "Audit access" });
        var deleted = await controller.DeleteRole(99);

        Assert.IsType<NotFoundObjectResult>(updated.Result);
        Assert.IsType<NotFoundObjectResult>(deleted.Result);
    }

    private sealed class FakeRoleService : IRoleService
    {
        private readonly bool _roleExists;

        public FakeRoleService(bool roleExists = true)
        {
            _roleExists = roleExists;
        }

        public Task<Result<Role>> GetAllRolesAsync() =>
            Task.FromResult(Success(null));

        public Task<Result<Role>> GetRoleByIdAsync(int id) =>
            Task.FromResult(_roleExists
                ? Success(new Role { Id = id, Name = "Auditor", Description = "Audit access" })
                : Failure("Role was not found."));

        public Task<Result<Role>> CreateRoleAsync(RoleDto roleDto, int createdBy) =>
            Task.FromResult(Success(new Role
            {
                Id = 5,
                Name = roleDto.Name,
                Description = roleDto.Description,
                CreatedBy = createdBy
            }));

        public Task<Result<Role>> UpdateRoleAsync(int id, RoleDto roleDto) =>
            Task.FromResult(_roleExists
                ? Success(new Role { Id = id, Name = roleDto.Name, Description = roleDto.Description })
                : Failure("Role was not found."));

        public Task<Result<Role>> DeleteRoleAsync(int id) =>
            Task.FromResult(_roleExists
                ? Success(new Role { Id = id })
                : Failure("Role was not found."));

        public Task SeedDefaultRolesAsync() => Task.CompletedTask;

        private static Result<Role> Success(Role? role) =>
            new()
            {
                Success = true,
                Data = role == null ? Array.Empty<Role>() : new[] { role }
            };

        private static Result<Role> Failure(string message) =>
            new()
            {
                Success = false,
                Message = message
            };
    }
}

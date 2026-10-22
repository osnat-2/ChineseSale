using System.Reflection;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Project;
using Project.Bll;
using Project.Bll.Interfaces;
using Project.Controllers;
using Project.Dal;
using Project.Dto;
using Project.Models;
using Xunit;

namespace Project.Tests;

public class UserManagementTests
{
    [Fact]
    public async Task CreateManagedUser_HashesPasswordAndStoresCreatorAndRole()
    {
        await using var context = CreateContext();
        var role = CreateRole(1, "User");
        context.Role.Add(role);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.CreateManagedUserAsync(new ManagedUserCreateDto
        {
            Name = "Managed User",
            Phone = "123456789",
            Email = "managed@example.com",
            Password = "secret1",
            RoleId = role.Id
        }, createdBy: 29);

        var user = Assert.Single(result.Data!);
        Assert.True(result.Success);
        Assert.Equal(29, user.CreatedBy);
        Assert.Equal(role.Id, user.RoleId);
        Assert.True(BCrypt.Net.BCrypt.Verify("secret1", user.Password));
        Assert.NotEqual("secret1", user.Password);
    }

    [Fact]
    public async Task ManagedUsers_ListIncludesInactiveButExcludesSoftDeleted()
    {
        await using var context = CreateContext();
        var role = CreateRole(1, "User");
        context.Role.Add(role);
        context.User.AddRange(
            CreateUser(1, "Active", role, active: true, deleted: false),
            CreateUser(2, "Inactive", role, active: false, deleted: false),
            CreateUser(3, "Deleted", role, active: false, deleted: true));
        await context.SaveChangesAsync();

        var result = await CreateDal(context).GetAllManagedUsersAsync();

        Assert.Equal(new[] { "Active", "Inactive" }, result.Data!.Select(user => user.Name));
    }

    [Fact]
    public async Task UpdateManagedUser_RejectsDeactivatingTheLastActiveAdmin()
    {
        await using var context = CreateContext();
        var adminRole = CreateRole(1, "Admin");
        context.Role.Add(adminRole);
        context.User.Add(CreateUser(1, "Only Admin", adminRole, active: true, deleted: false));
        await context.SaveChangesAsync();
        var dal = CreateDal(context);

        var result = await dal.UpdateManagedUserAsync(1, new User
        {
            Name = "Only Admin",
            Phone = "123456789",
            Email = "user1@example.com",
            RoleId = adminRole.Id,
            Role = adminRole,
            IsActive = false
        });

        Assert.False(result.Success);
        Assert.Contains("last active administrator", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.True((await context.User.SingleAsync()).IsActive);
    }

    [Fact]
    public async Task DeleteManagedUser_RejectsDeletingLastAdminAndReferencedDonor()
    {
        await using var context = CreateContext();
        var adminRole = CreateRole(1, "Admin");
        var donorRole = CreateRole(2, "Donor");
        var category = new Category
        {
            Id = 1,
            Name = "Gifts",
            IsActive = true,
            IsDeleted = false
        };
        context.Role.AddRange(adminRole, donorRole);
        context.Category.Add(category);
        context.User.AddRange(
            CreateUser(1, "Only Admin", adminRole, active: true, deleted: false),
            CreateUser(2, "Donor", donorRole, active: true, deleted: false));
        context.Present.Add(new Present
        {
            Id = 1,
            Name = "Gift",
            Description = "Donated gift",
            DonorId = 2,
            CategoryId = 1,
            ImageUrl = "https://example.com/gift.jpg",
            Quantity = 1,
            Price = 10,
            IsActive = false,
            IsDeleted = true
        });
        await context.SaveChangesAsync();
        var dal = CreateDal(context);

        var lastAdmin = await dal.DeleteManagedUserAsync(1);
        var referencedDonor = await dal.DeleteManagedUserAsync(2);

        Assert.False(lastAdmin.Success);
        Assert.Contains("last active administrator", lastAdmin.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(referencedDonor.Success);
        Assert.Contains("referenced by presents", referencedDonor.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False((await context.User.IgnoreQueryFilters().SingleAsync(user => user.Id == 2)).IsDeleted);
    }

    [Fact]
    public void UserManagementController_RequiresAdminForAllEndpoints()
    {
        var authorize = typeof(UserManagementController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authorize);
        Assert.Equal("Admin", authorize.Roles);
    }

    [Fact]
    public async Task GetUsers_UsesPasswordFreeResponseDto()
    {
        var controller = new UserManagementController(new StubUserService(
            new User
            {
                Id = 8,
                Name = "Managed User",
                Phone = "123456789",
                Email = "managed@example.com",
                Password = "secret-hash",
                RoleId = 2,
                Role = CreateRole(2, "User"),
                IsActive = true
            }));

        var action = await controller.GetUsers();
        var response = Assert.IsType<OkObjectResult>(action.Result);
        var result = Assert.IsType<Result<ManagedUserResponseDto>>(response.Value);
        var user = Assert.Single(result.Data!);

        Assert.Equal("User", user.RoleName);
        Assert.DoesNotContain(
            typeof(ManagedUserResponseDto).GetProperties(),
            property => property.Name.Contains("password", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task DeleteUser_RejectsDeletingTheCurrentAccount()
    {
        var controller = new UserManagementController(new StubUserService(new User()))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, "8")
                    ], "test"))
                }
            }
        };

        var action = await controller.DeleteUser(8);

        Assert.IsType<BadRequestObjectResult>(action.Result);
    }

    [Fact]
    public async Task CreateUser_RejectsNullRequest()
    {
        var controller = new UserManagementController(new StubUserService(new User()));

        var action = await controller.CreateUser(null);

        Assert.IsType<BadRequestObjectResult>(action.Result);
    }

    [Fact]
    public async Task UpdateUser_RejectsNullRequest()
    {
        var controller = new UserManagementController(new StubUserService(new User()));

        var action = await controller.UpdateUser(8, null);

        Assert.IsType<BadRequestObjectResult>(action.Result);
    }

    private static AppDBContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDBContext(options);
    }

    private static UserDal CreateDal(AppDBContext context) =>
        new(context, NullLogger<UserDal>.Instance);

    private static UserService CreateService(AppDBContext context) =>
        new(
            Options.Create(new JWTSettings()),
            CreateDal(context),
            new MapperConfiguration(_ => { }).CreateMapper(),
            NullLogger<UserService>.Instance);

    private static Role CreateRole(int id, string name) =>
        new()
        {
            Id = id,
            Name = name,
            Description = $"{name} role",
            IsActive = true
        };

    private static User CreateUser(int id, string name, Role role, bool active, bool deleted) =>
        new()
        {
            Id = id,
            Name = name,
            Phone = "123456789",
            Email = $"user{id}@example.com",
            Password = "hashed",
            RoleId = role.Id,
            Role = role,
            IsActive = active,
            IsDeleted = deleted
        };

    private sealed class StubUserService(User user) : IUserService
    {
        private Result<User> Result() => new()
        {
            Success = true,
            Data = new[] { user }
        };

        public Task<Result<string>> Login(string email, string password) =>
            Task.FromResult(new Result<string> { Success = true });
        public Task<Result<User>> Register(UserDto userDto) => Task.FromResult(Result());
        public Task<Result<User>> AddDonor(UserDto userDto) => Task.FromResult(Result());
        public Task<Result<User>> AddAdmin(UserDto userDto) => Task.FromResult(Result());
        public Task<Result<User>> GetDonorsAsync() => Task.FromResult(Result());
        public Task<Result<User>> UpdateDonorAsync(int id, DonorUpdateDto donorDto) => Task.FromResult(Result());
        public Task<Result<User>> DeleteDonorAsync(int id) => Task.FromResult(Result());
        public Task<Result<User>> GetAllManagedUsersAsync() => Task.FromResult(Result());
        public Task<Result<User>> CreateManagedUserAsync(ManagedUserCreateDto userDto, int createdBy) =>
            Task.FromResult(Result());
        public Task<Result<User>> UpdateManagedUserAsync(int id, ManagedUserUpdateDto userDto) =>
            Task.FromResult(Result());
        public Task<Result<User>> DeleteManagedUserAsync(int id) => Task.FromResult(Result());
    }
}

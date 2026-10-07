using Microsoft.AspNetCore.Mvc;
using Project.Bll.Interfaces;
using Project.Controllers;
using Project.Dto;
using Project.Models;
using Xunit;

namespace Project.Tests;

public class UserControllerTests
{
    [Theory]
    [InlineData("register")]
    [InlineData("donor")]
    [InlineData("admin")]
    public async Task UserCreationResponses_DoNotExposePasswordHash(string operation)
    {
        var user = new User
        {
            Id = 42,
            Name = "Test User",
            Phone = "123456789",
            Email = "test@example.com",
            Password = "$2a$12$stored-bcrypt-hash",
            RoleId = 1,
            IsActive = true,
            IsDeleted = false
        };
        var service = new FakeUserService(user);
        var controller = new UserController(service);

        var actionResult = operation switch
        {
            "register" => await controller.Register(new UserDto()),
            "donor" => await controller.AddDonor(new UserDto()),
            _ => await controller.AddAdmin(new UserDto())
        };

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var safeResult = Assert.IsType<Result<UserResponseDto>>(okResult.Value);
        var responseUser = Assert.Single(safeResult.Data!);

        Assert.Equal(user.Id, responseUser.Id);
        Assert.Equal(user.Email, responseUser.Email);
        Assert.DoesNotContain(
            typeof(UserResponseDto).GetProperties(),
            property => property.Name.Contains("password", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class FakeUserService(User user) : IUserService
    {
        private Result<User> SuccessResult() => new()
        {
            Success = true,
            Message = "User created.",
            Data = new[] { user }
        };

        public Task<Result<string>> Login(string email, string password) =>
            Task.FromResult(new Result<string> { Success = true });

        public Task<Result<User>> Register(UserDto userDto) => Task.FromResult(SuccessResult());

        public Task<Result<User>> AddDonor(UserDto userDto) => Task.FromResult(SuccessResult());

        public Task<Result<User>> AddAdmin(UserDto userDto) => Task.FromResult(SuccessResult());

        public Task<Result<User>> GetDonorsAsync() => Task.FromResult(SuccessResult());

        public Task<Result<User>> UpdateDonorAsync(int id, DonorUpdateDto donorDto) =>
            Task.FromResult(SuccessResult());

        public Task<Result<User>> DeleteDonorAsync(int id) => Task.FromResult(SuccessResult());
    }
}

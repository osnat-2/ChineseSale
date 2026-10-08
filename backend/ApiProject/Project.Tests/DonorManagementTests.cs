using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Project;
using Project.Bll.Interfaces;
using Project.Controllers;
using Project.Dal;
using Project.Dto;
using Project.Models;
using Xunit;

namespace Project.Tests;

public class DonorManagementTests
{
    [Fact]
    public async Task GetDonors_ReturnsOnlyDonorRoleUsers()
    {
        await using var dbContext = CreateContext();
        var donorRole = new Role { Id = 1, Name = "Donor", Description = "Gift donor", IsActive = true };
        var userRole = new Role { Id = 2, Name = "User", Description = "Regular account", IsActive = true };
        dbContext.Role.AddRange(donorRole, userRole);
        dbContext.User.AddRange(
            CreateUser(1, "Donor User", "donor@example.com", donorRole),
            CreateUser(2, "Regular User", "user@example.com", userRole));
        await dbContext.SaveChangesAsync();

        var result = await CreateDal(dbContext).GetDonorsAsync();

        Assert.True(result.Success);
        var donor = Assert.Single(result.Data!);
        Assert.Equal("Donor User", donor.Name);
    }

    [Fact]
    public async Task UpdateDonor_ChangesContactDetailsWithoutChangingPassword()
    {
        await using var dbContext = CreateContext();
        var donorRole = new Role { Id = 1, Name = "Donor", Description = "Gift donor", IsActive = true };
        dbContext.Role.Add(donorRole);
        var donor = CreateUser(1, "Old Name", "old@example.com", donorRole);
        donor.Password = "existing-password-hash";
        dbContext.User.Add(donor);
        await dbContext.SaveChangesAsync();

        var result = await CreateDal(dbContext).UpdateDonorAsync(1, new User
        {
            Name = "New Name",
            Phone = "123456789",
            Email = "new@example.com"
        });

        Assert.True(result.Success);
        var updated = Assert.Single(result.Data!);
        Assert.Equal("New Name", updated.Name);
        Assert.Equal("new@example.com", updated.Email);
        Assert.Equal("existing-password-hash", updated.Password);
    }

    [Fact]
    public async Task UpdateDonor_RejectsNonDonorsAndDuplicateEmails()
    {
        await using var dbContext = CreateContext();
        var donorRole = new Role { Id = 1, Name = "Donor", Description = "Gift donor", IsActive = true };
        var userRole = new Role { Id = 2, Name = "User", Description = "Regular account", IsActive = true };
        dbContext.Role.AddRange(donorRole, userRole);
        dbContext.User.AddRange(
            CreateUser(1, "First Donor", "first@example.com", donorRole),
            CreateUser(2, "Second Donor", "second@example.com", donorRole),
            CreateUser(3, "Regular User", "user@example.com", userRole));
        await dbContext.SaveChangesAsync();

        var dal = CreateDal(dbContext);
        var notDonor = await dal.UpdateDonorAsync(3, new User
        {
            Name = "Changed User",
            Phone = "123456789",
            Email = "changed@example.com"
        });
        var duplicateEmail = await dal.UpdateDonorAsync(1, new User
        {
            Name = "Changed Donor",
            Phone = "123456789",
            Email = "second@example.com"
        });

        Assert.False(notDonor.Success);
        Assert.Equal("Donor was not found.", notDonor.Message);
        Assert.False(duplicateEmail.Success);
        Assert.Equal("Email already exists.", duplicateEmail.Message);
    }

    [Fact]
    public async Task DeleteDonor_SoftDeletesAndRemovesDonorFromNormalList()
    {
        await using var dbContext = CreateContext();
        var donorRole = new Role { Id = 1, Name = "Donor", Description = "Gift donor", IsActive = true };
        dbContext.Role.Add(donorRole);
        dbContext.User.Add(CreateUser(1, "Donor User", "donor@example.com", donorRole));
        await dbContext.SaveChangesAsync();

        var dal = CreateDal(dbContext);
        var deleteResult = await dal.DeleteDonorAsync(1);
        var donorRecord = await dbContext.User.IgnoreQueryFilters().SingleAsync();
        var donorsAfterDelete = await dal.GetDonorsAsync();

        Assert.True(deleteResult.Success);
        Assert.False(donorRecord.IsActive);
        Assert.True(donorRecord.IsDeleted);
        Assert.NotNull(donorRecord.DeletedAt);
        Assert.Empty(donorsAfterDelete.Data!);
    }

    [Fact]
    public async Task DeleteDonor_RejectsDonorsReferencedByPresents()
    {
        await using var dbContext = CreateContext();
        var donorRole = new Role { Id = 1, Name = "Donor", Description = "Gift donor", IsActive = true };
        dbContext.Role.Add(donorRole);
        dbContext.User.Add(CreateUser(1, "Donor User", "donor@example.com", donorRole));
        dbContext.Present.Add(new Present
        {
            Id = 1,
            Name = "Gift",
            Description = "Gift description",
            DonorId = 1,
            CategoryId = 1,
            ImageUrl = "https://example.com/gift.jpg",
            Quantity = 1,
            Price = 10,
            IsActive = true
        });
        await dbContext.SaveChangesAsync();

        var result = await CreateDal(dbContext).DeleteDonorAsync(1);
        var donorRecord = await dbContext.User.SingleAsync();

        Assert.False(result.Success);
        Assert.Contains("assigned", result.Message);
        Assert.False(donorRecord.IsDeleted);
    }

    [Fact]
    public void DonorController_RequiresAdminForAllEndpoints()
    {
        var authorize = typeof(DonorController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        Assert.Equal("Admin", authorize.Roles);
    }

    [Fact]
    public async Task GetDonors_ReturnsSafeResponseWithoutPassword()
    {
        var donor = new User
        {
            Id = 4,
            Name = "Donor User",
            Phone = "123456789",
            Email = "donor@example.com",
            Password = "stored-password-hash",
            IsActive = true
        };
        var controller = new DonorController(new StubUserService(donor));

        var action = await controller.GetDonors();

        var response = Assert.IsType<OkObjectResult>(action.Result);
        var result = Assert.IsType<Result<UserResponseDto>>(response.Value);
        var donorResponse = Assert.Single(result.Data!);
        Assert.Equal(donor.Id, donorResponse.Id);
        Assert.True(donorResponse.IsActive);
        Assert.DoesNotContain(
            typeof(UserResponseDto).GetProperties(),
            property => property.Name.Contains("password", StringComparison.OrdinalIgnoreCase));
    }

    private static AppDBContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDBContext(options);
    }

    private static UserDal CreateDal(AppDBContext dbContext) =>
        new(dbContext, NullLogger<UserDal>.Instance);

    private static User CreateUser(int id, string name, string email, Role role) =>
        new()
        {
            Id = id,
            Name = name,
            Phone = "123456789",
            Email = email,
            Password = "password-hash",
            RoleId = role.Id,
            Role = role,
            IsActive = true
        };

    private sealed class StubUserService(User donor) : IUserService
    {
        private Result<User> DonorResult() => new()
        {
            Success = true,
            Data = new[] { donor }
        };

        public Task<Result<string>> Login(string email, string password) =>
            Task.FromResult(new Result<string> { Success = true });

        public Task<Result<User>> Register(UserDto userDto) => Task.FromResult(DonorResult());

        public Task<Result<User>> AddDonor(UserDto userDto) => Task.FromResult(DonorResult());

        public Task<Result<User>> AddAdmin(UserDto userDto) => Task.FromResult(DonorResult());

        public Task<Result<User>> GetDonorsAsync() => Task.FromResult(DonorResult());

        public Task<Result<User>> UpdateDonorAsync(int id, DonorUpdateDto donorDto) =>
            Task.FromResult(DonorResult());

        public Task<Result<User>> DeleteDonorAsync(int id) => Task.FromResult(DonorResult());
    }
}

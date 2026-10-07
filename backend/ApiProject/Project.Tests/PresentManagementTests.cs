using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Project;
using Project.Dal;
using Project.Models;
using Xunit;

namespace Project.Tests;

public class PresentManagementTests
{
    [Fact]
    public async Task GetAllPresents_OnlyIncludesInactiveWhenRequested()
    {
        await using var dbContext = CreateContext();
        var role = new Role { Id = 1, Name = "Donor", Description = "Gift donor", IsActive = true };
        dbContext.Role.Add(role);
        dbContext.User.Add(new User
        {
            Id = 1,
            Name = "Donor",
            Phone = "123456789",
            Email = "donor@example.com",
            RoleId = role.Id,
            Role = role,
            IsActive = true
        });
        dbContext.Present.AddRange(
            CreatePresent(1, "Active present", isActive: true),
            CreatePresent(2, "Inactive present", isActive: false));
        await dbContext.SaveChangesAsync();

        var dal = CreateDal(dbContext);
        var activeOnly = await dal.GetAllPresentsAsync(onlyActive: true);
        var allActiveStates = await dal.GetAllPresentsAsync(onlyActive: false);

        Assert.Single(activeOnly.Data!);
        Assert.Equal(2, allActiveStates.Data!.Count());
    }

    [Fact]
    public async Task UpdatePresent_PersistsImageAndPrice()
    {
        await using var dbContext = CreateContext();
        var role = new Role { Id = 1, Name = "Donor", Description = "Gift donor", IsActive = true };
        dbContext.Role.Add(role);
        dbContext.User.Add(new User
        {
            Id = 1,
            Name = "Donor",
            Phone = "123456789",
            Email = "donor@example.com",
            RoleId = role.Id,
            Role = role,
            IsActive = true
        });
        var present = CreatePresent(1, "Gift", isActive: true);
        dbContext.Present.Add(present);
        await dbContext.SaveChangesAsync();

        var result = await CreateDal(dbContext).UpdatePresentAsync(new Present
        {
            Id = present.Id,
            Name = present.Name,
            DonorId = present.DonorId,
            CategoryId = present.CategoryId,
            Quantity = present.Quantity,
            Price = 25,
            Description = "Updated description",
            ImageUrl = "https://example.com/updated.jpg"
        });

        var updated = await dbContext.Present.SingleAsync();
        Assert.True(result.Success);
        Assert.Equal(25, updated.Price);
        Assert.Equal("https://example.com/updated.jpg", updated.ImageUrl);
        Assert.NotNull(updated.UpdatedAt);
    }

    private static AppDBContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDBContext(options);
    }

    private static PresentDal CreateDal(AppDBContext dbContext) =>
        new(dbContext, NullLogger<PresentDal>.Instance);

    private static Present CreatePresent(int id, string name, bool isActive) =>
        new()
        {
            Id = id,
            Name = name,
            Description = "Description",
            DonorId = 1,
            CategoryId = 1,
            Quantity = 1,
            Price = 10,
            ImageUrl = "https://example.com/present.jpg",
            IsActive = isActive
        };
}

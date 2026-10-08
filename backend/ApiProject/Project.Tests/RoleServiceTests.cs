using Microsoft.EntityFrameworkCore;
using Project.Bll;
using Project.Dal;
using Project.Dto;
using Project.Models;
using Xunit;

namespace Project.Tests;

public class RoleServiceTests
{
    [Fact]
    public async Task SeedDefaultRolesAsync_WhenNoRolesExist_CreatesAllBuiltInRolesAsActive()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        await service.SeedDefaultRolesAsync();

        var roles = await context.Role.OrderBy(role => role.Name).ToListAsync();
        Assert.Equal(new[] { "Admin", "Donor", "User" }, roles.Select(role => role.Name));
        Assert.All(roles, role =>
        {
            Assert.True(role.IsActive);
            Assert.False(role.IsDeleted);
            Assert.Equal(0, role.CreatedBy);
        });
    }

    [Fact]
    public async Task SeedDefaultRolesAsync_IsIdempotentAndReactivatesExistingBuiltInRole()
    {
        await using var context = CreateContext();
        var originalCreatedAt = DateTime.UtcNow.AddDays(-10);
        var originalDescription = "Preserve this description";
        context.Role.Add(new Role
        {
            Id = 10,
            Name = "user",
            Description = originalDescription,
            IsActive = false,
            IsDeleted = true,
            CreatedAt = originalCreatedAt,
            DeletedAt = DateTime.UtcNow.AddDays(-2),
            CreatedBy = 42
        });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        await service.SeedDefaultRolesAsync();
        await service.SeedDefaultRolesAsync();

        var roles = await context.Role.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(3, roles.Count);
        var userRole = Assert.Single(roles, role => role.Name == "User");
        Assert.True(userRole.IsActive);
        Assert.False(userRole.IsDeleted);
        Assert.Null(userRole.DeletedAt);
        Assert.Equal(originalDescription, userRole.Description);
        Assert.Equal(originalCreatedAt, userRole.CreatedAt);
        Assert.Equal(42, userRole.CreatedBy);
    }

    [Fact]
    public async Task CreateUpdateAndDeleteRoleAsync_CreatesUpdatesAndSoftDeletesCustomRole()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var created = await service.CreateRoleAsync(
            new RoleDto { Name = "Auditor", Description = "Audit access" },
            createdBy: 9);
        var role = Assert.Single(created.Data!);
        Assert.True(created.Success);
        Assert.Equal(9, role.CreatedBy);

        var updated = await service.UpdateRoleAsync(role.Id, new RoleDto
        {
            Name = "Auditor",
            Description = "Updated description",
            IsActive = true
        });

        Assert.True(updated.Success);
        Assert.Equal("Updated description", role.Description);

        var deleted = await service.DeleteRoleAsync(role.Id);

        Assert.True(deleted.Success);
        Assert.False(role.IsActive);
        Assert.True(role.IsDeleted);
        Assert.NotNull(role.DeletedAt);
        Assert.Null(await context.Role.FirstOrDefaultAsync(item => item.Id == role.Id));
    }

    [Fact]
    public async Task UpdateAndDeleteRoleAsync_RejectsChangesToBuiltInRoles()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        await service.SeedDefaultRolesAsync();
        var admin = await context.Role.SingleAsync(role => role.Name == "Admin");

        var rename = await service.UpdateRoleAsync(admin.Id, new RoleDto
        {
            Name = "SuperAdmin",
            Description = admin.Description,
            IsActive = true
        });
        var deactivate = await service.UpdateRoleAsync(admin.Id, new RoleDto
        {
            Name = "Admin",
            Description = admin.Description,
            IsActive = false
        });
        var delete = await service.DeleteRoleAsync(admin.Id);

        Assert.False(rename.Success);
        Assert.False(deactivate.Success);
        Assert.False(delete.Success);
        Assert.True(admin.IsActive);
        Assert.False(admin.IsDeleted);
    }

    [Fact]
    public async Task UpdateAndDeleteRoleAsync_RejectsDisablingOrDeletingRoleAssignedToSoftDeletedUser()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var created = await service.CreateRoleAsync(
            new RoleDto { Name = "Auditor", Description = "Audit access" },
            createdBy: 9);
        var role = Assert.Single(created.Data!);
        context.User.Add(new User
        {
            Id = 1,
            Name = "Former user",
            Phone = "123",
            Email = "former@example.com",
            Password = "hashed",
            RoleId = role.Id,
            IsActive = false,
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var deactivate = await service.UpdateRoleAsync(role.Id, new RoleDto
        {
            Name = role.Name,
            Description = role.Description,
            IsActive = false
        });
        var delete = await service.DeleteRoleAsync(role.Id);

        Assert.False(deactivate.Success);
        Assert.False(delete.Success);
        Assert.False(role.IsDeleted);
    }

    [Fact]
    public async Task CreateRoleAsync_RejectsDuplicateNamesIgnoringCase()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        await service.CreateRoleAsync(
            new RoleDto { Name = "Auditor", Description = "Audit access" },
            createdBy: 9);

        var duplicate = await service.CreateRoleAsync(
            new RoleDto { Name = " auditor ", Description = "Duplicate" },
            createdBy: 10);

        Assert.False(duplicate.Success);
        Assert.Equal(1, await context.Role.CountAsync());
    }

    private static AppDBContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDBContext(options);
    }

    private static RoleService CreateService(AppDBContext context) =>
        new(new RoleDal(context));
}

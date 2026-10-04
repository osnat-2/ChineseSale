using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.Bll;
using Project.Bll.Interfaces;
using Project.Dal;
using Project.Models;
using Xunit;

namespace Project.Tests;

public class LotteryServiceTests
{
    [Fact]
    public async Task DrawWinnerForPresentAsync_WhenPresentHasNoPaidCards_ReturnsFriendlyValidationError()
    {
        var options = new DbContextOptionsBuilder<AppDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDBContext(options);

        var role = new Role { Id = 1, Name = "User", Description = "Standard user role", IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        var donor = new User
        {
            Id = 1,
            Name = "Donor",
            Email = "donor@test.com",
            Phone = "123",
            Password = "pw",
            RoleId = 1,
            Role = role,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var present = new Present
        {
            Id = 5,
            Name = "No Winner Prize",
            Description = "Valid present with no paid cards",
            DonorId = donor.Id,
            Donor = donor,
            CategoryId = 1,
            ImageUrl = "img",
            Quantity = 3,
            Price = 25,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = donor.Id
        };

        context.Role.Add(role);
        context.User.Add(donor);
        context.Present.Add(present);
        await context.SaveChangesAsync();

        var logger = new LoggerFactory().CreateLogger<WinnerDal>();
        var winnerDal = new WinnerDal(context, logger);
        var service = new WinnerService(winnerDal, new FakeEmailService());

        var result = await service.DrawWinnerForPresentAsync(present.Id);

        Assert.False(result.Success);
        Assert.Contains("paid cards", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DrawWinnerForPresentAsync_WhenPaidCardsExist_CreatesWinnerAndMarksLotteryCompleteAtomically()
    {
        var options = new DbContextOptionsBuilder<AppDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDBContext(options);

        var role = new Role { Id = 1, Name = "User", Description = "Standard user role", IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        var donor = new User { Id = 1, Name = "Donor", Email = "donor@test.com", Phone = "123", Password = "pw", RoleId = 1, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        var present = new Present { Id = 10, Name = "Gift", Description = "Gift", DonorId = donor.Id, Donor = donor, CategoryId = 1, ImageUrl = "img", Quantity = 5, Price = 10, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow, CreatedBy = donor.Id };
        var paidCard = new Card { Id = 20, PresentId = present.Id, Present = present, IsPaid = true, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow, CreatedBy = donor.Id };
        var lottery = new Lottery { Id = 30, Time = DateOnly.FromDateTime(DateTime.UtcNow), IsMadeOut = false, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow, CreatedBy = donor.Id };

        context.Role.Add(role);
        context.User.Add(donor);
        context.Present.Add(present);
        context.Card.Add(paidCard);
        context.Lottery.Add(lottery);
        await context.SaveChangesAsync();

        var winnerDal = new WinnerDal(context, new LoggerFactory().CreateLogger<WinnerDal>());
        var service = new WinnerService(winnerDal, new FakeEmailService());

        var result = await service.DrawWinnerForPresentAsync(present.Id, lottery.Id);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(context.Winner);
        Assert.True(context.Lottery.Single(x => x.Id == lottery.Id).IsMadeOut);
    }

    private sealed class FakeEmailService : IEmailService
    {
    }
}

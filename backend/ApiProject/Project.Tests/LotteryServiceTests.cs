using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.Bll;
using Project.Bll.Interfaces;
using Project.Dal;
using Project.Dto;
using Project.Json;
using Project.Models;
using Newtonsoft.Json;
using Xunit;

namespace Project.Tests;

public class LotteryServiceTests
{
    [Fact]
    public void LotteryDateJsonConverter_UsesIsoDateOnlyContract()
    {
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new DateOnlyJsonConverter());
        const string json = "\"2026-11-01\"";

        var date = JsonConvert.DeserializeObject<DateOnly>(json, settings);
        var serialized = JsonConvert.SerializeObject(date, settings);
        var lotteryDto = JsonConvert.DeserializeObject<LotteryDto>("{\"time\":\"2026-11-01\"}", settings);

        Assert.Equal(new DateOnly(2026, 11, 1), date);
        Assert.Equal(json, serialized);
        Assert.Equal(new DateOnly(2026, 11, 1), lotteryDto!.Time);
    }

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
        var lottery = new Lottery
        {
            Id = 1,
            Time = new DateOnly(DateTime.UtcNow.Year, 10, 1),
            IsMadeOut = false,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Role.Add(role);
        context.User.Add(donor);
        context.Present.Add(present);
        context.Lottery.Add(lottery);
        await context.SaveChangesAsync();

        var logger = new LoggerFactory().CreateLogger<WinnerDal>();
        var winnerDal = new WinnerDal(context, logger);
        var service = new WinnerService(winnerDal, new FakeEmailService());

        var result = await service.DrawWinnerForPresentAsync(present.Id, lottery.Id);

        Assert.False(result.Success);
        Assert.Contains("paid cards", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DrawWinnerForPresentAsync_WhenPaidCardsAreFewerThanWinnerCount_RejectsDraw()
    {
        var options = new DbContextOptionsBuilder<AppDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDBContext(options);
        var role = new Role { Id = 1, Name = "User", Description = "Standard user role", IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        var donor = new User { Id = 1, Name = "Donor", Email = "donor@test.com", Phone = "123", Password = "pw", RoleId = 1, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        var present = new Present { Id = 11, Name = "Multiple Winners", Description = "Gift", DonorId = donor.Id, Donor = donor, CategoryId = 1, ImageUrl = "img", Quantity = 2, Price = 10, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow, CreatedBy = donor.Id };
        var lottery = new Lottery { Id = 1, Time = new DateOnly(DateTime.UtcNow.Year, 10, 1), IsMadeOut = false, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        var paidCard = new Card { Id = 21, PresentId = present.Id, Present = present, IsPaid = true, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow, CreatedBy = donor.Id };
        context.Role.Add(role);
        context.User.Add(donor);
        context.Present.Add(present);
        context.Lottery.Add(lottery);
        context.Card.Add(paidCard);
        await context.SaveChangesAsync();

        var service = new WinnerService(
            new WinnerDal(context, new LoggerFactory().CreateLogger<WinnerDal>()),
            new FakeEmailService());

        var result = await service.DrawWinnerForPresentAsync(present.Id, lottery.Id);

        Assert.False(result.Success);
        Assert.Contains("2 distinct paid cards", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(context.Winner);
        Assert.False(Assert.Single(context.Lottery).IsMadeOut);
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
        var present = new Present { Id = 10, Name = "Gift", Description = "Gift", DonorId = donor.Id, Donor = donor, CategoryId = 1, ImageUrl = "img", Quantity = 2, Price = 10, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow, CreatedBy = donor.Id };
        var lottery = new Lottery { Id = 1, Time = new DateOnly(DateTime.UtcNow.Year, 10, 1), IsMadeOut = false, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        var paidCards = new[]
        {
            new Card { Id = 20, PresentId = present.Id, Present = present, IsPaid = true, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow, CreatedBy = donor.Id },
            new Card { Id = 21, PresentId = present.Id, Present = present, IsPaid = true, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow, CreatedBy = donor.Id },
            new Card { Id = 22, PresentId = present.Id, Present = present, IsPaid = true, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow, CreatedBy = donor.Id }
        };
        context.Role.Add(role);
        context.User.Add(donor);
        context.Present.Add(present);
        context.Lottery.Add(lottery);
        context.Card.AddRange(paidCards);
        await context.SaveChangesAsync();

        var winnerDal = new WinnerDal(context, new LoggerFactory().CreateLogger<WinnerDal>());
        var service = new WinnerService(winnerDal, new FakeEmailService());

        var result = await service.DrawWinnerForPresentAsync(present.Id, lottery.Id);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(present.Quantity, result.Data.Count());
        Assert.Equal(present.Quantity, context.Winner.Count());
        Assert.Equal(present.Quantity, result.Data.Select(winner => winner.CardId).Distinct().Count());
        var completedLottery = Assert.Single(context.Lottery);
        Assert.True(completedLottery.IsMadeOut);
        Assert.All(result.Data, winner => Assert.Equal(completedLottery.Id, winner.LotteryId));

        var repeatResult = await service.DrawWinnerForPresentAsync(present.Id, lottery.Id);

        Assert.False(repeatResult.Success);
        Assert.Contains("already been drawn", repeatResult.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(present.Quantity, context.Winner.Count());

        var nextYearLottery = new Lottery
        {
            Time = new DateOnly(lottery.Time.Year + 1, lottery.Time.Month, lottery.Time.Day),
            IsMadeOut = false,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        context.Lottery.Add(nextYearLottery);
        await context.SaveChangesAsync();
        var nextYearResult = await service.DrawWinnerForPresentAsync(present.Id, nextYearLottery.Id);
        Assert.True(nextYearResult.Success);
        Assert.Equal(2, context.Winner.Count());
    }

    [Fact]
    public async Task ProcessLotteryAsync_WhenEveryActivePresentHasEnoughPaidCards_DrawsAllPresentsTogether()
    {
        var options = new DbContextOptionsBuilder<AppDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new AppDBContext(options);
        var role = new Role { Id = 1, Name = "User", Description = "User", IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        var donor = new User { Id = 1, Name = "Donor", Email = "donor@test.com", Phone = "123", Password = "pw", RoleId = 1, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        var firstPresent = CreatePresent(31, 2, donor.Id);
        var secondPresent = CreatePresent(32, 1, donor.Id);
        var lottery = CreateLottery(DateTime.UtcNow.Year);
        context.Role.Add(role);
        context.User.Add(donor);
        context.Present.AddRange(firstPresent, secondPresent);
        context.Lottery.Add(lottery);
        context.Card.AddRange(
            CreateCard(41, firstPresent, donor.Id),
            CreateCard(42, firstPresent, donor.Id),
            CreateCard(43, secondPresent, donor.Id));
        await context.SaveChangesAsync();

        var service = CreateWinnerService(context);
        var result = await service.ProcessLotteryAsync(lottery.Id);

        Assert.True(result.Success);
        Assert.Equal(3, result.Data!.Count());
        Assert.Equal(2, result.Data.Count(winner => winner.PresentId == firstPresent.Id));
        Assert.Single(result.Data.Where(winner => winner.PresentId == secondPresent.Id));
        Assert.Equal(3, result.Data.Select(winner => winner.CardId).Distinct().Count());
        Assert.True((await context.Lottery.SingleAsync()).IsMadeOut);

        var repeat = await service.ProcessLotteryAsync(lottery.Id);
        Assert.False(repeat.Success);
        Assert.Contains("already been completed", repeat.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcessLotteryAsync_WhenAnyPresentLacksPaidCards_SavesNoPartialDraw()
    {
        var options = new DbContextOptionsBuilder<AppDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new AppDBContext(options);
        var role = new Role { Id = 1, Name = "User", Description = "User", IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        var donor = new User { Id = 1, Name = "Donor", Email = "donor@test.com", Phone = "123", Password = "pw", RoleId = 1, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        var fundedPresent = CreatePresent(51, 1, donor.Id);
        var underfundedPresent = CreatePresent(52, 2, donor.Id);
        var lottery = CreateLottery(DateTime.UtcNow.Year);
        context.Role.Add(role);
        context.User.Add(donor);
        context.Present.AddRange(fundedPresent, underfundedPresent);
        context.Lottery.Add(lottery);
        context.Card.AddRange(
            CreateCard(61, fundedPresent, donor.Id),
            CreateCard(62, underfundedPresent, donor.Id));
        await context.SaveChangesAsync();

        var result = await CreateWinnerService(context).ProcessLotteryAsync(lottery.Id);

        Assert.False(result.Success);
        Assert.Contains("requires 2 more distinct paid cards", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(context.Winner);
        Assert.False((await context.Lottery.SingleAsync()).IsMadeOut);
    }

    [Fact]
    public async Task ProcessLotteryAsync_WhenThereAreNoActivePresents_DoesNotCompleteLottery()
    {
        var options = new DbContextOptionsBuilder<AppDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new AppDBContext(options);
        var lottery = CreateLottery(DateTime.UtcNow.Year);
        context.Lottery.Add(lottery);
        await context.SaveChangesAsync();

        var result = await CreateWinnerService(context).ProcessLotteryAsync(lottery.Id);

        Assert.False(result.Success);
        Assert.Contains("no active presents", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False((await context.Lottery.SingleAsync()).IsMadeOut);
    }

    [Fact]
    public async Task AddLotteryAsync_RejectsDuplicateCalendarYear()
    {
        var options = new DbContextOptionsBuilder<AppDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new AppDBContext(options);
        var dal = new LotteryDal(context, new LoggerFactory().CreateLogger<LotteryDal>());

        var first = await dal.AddLotteryAsync(CreateLottery(DateTime.UtcNow.Year));
        var duplicate = await dal.AddLotteryAsync(CreateLottery(DateTime.UtcNow.Year));
        var nextYear = await dal.AddLotteryAsync(CreateLottery(DateTime.UtcNow.Year + 1));

        Assert.True(first.Success);
        Assert.False(duplicate.Success);
        Assert.True(nextYear.Success);
        var moveIntoDuplicateYear = await dal.UpdateLotteryAsync(
            first.Data!.Single().Id,
            new DateOnly(DateTime.UtcNow.Year + 1, 11, 1));
        Assert.False(moveIntoDuplicateYear.Success);
        Assert.Equal(2, context.Lottery.Count());
    }

    [Fact]
    public async Task UpdateLotteryAsync_RejectsDateChangeAfterWinnersExist()
    {
        var options = new DbContextOptionsBuilder<AppDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new AppDBContext(options);
        var lottery = CreateLottery(DateTime.UtcNow.Year);
        var role = new Role { Id = 1, Name = "User", Description = "User", IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        var donor = new User { Id = 1, Name = "Donor", Email = "donor@test.com", Phone = "123", Password = "pw", RoleId = 1, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow };
        var present = CreatePresent(71, 1, donor.Id);
        var card = CreateCard(81, present, donor.Id);
        context.Role.Add(role);
        context.User.Add(donor);
        context.Present.Add(present);
        context.Lottery.Add(lottery);
        context.Card.Add(card);
        await context.SaveChangesAsync();
        context.Winner.Add(new Winner
        {
            PresentId = present.Id,
            CardId = card.Id,
            LotteryId = lottery.Id,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        var dal = new LotteryDal(context, new LoggerFactory().CreateLogger<LotteryDal>());

        var update = await dal.UpdateLotteryAsync(lottery.Id, new DateOnly(lottery.Time.Year, 11, 1));

        Assert.False(update.Success);
        Assert.Equal(new DateOnly(DateTime.UtcNow.Year, 10, 1), (await context.Lottery.SingleAsync()).Time);
    }

    private static WinnerService CreateWinnerService(AppDBContext context) =>
        new(new WinnerDal(context, new LoggerFactory().CreateLogger<WinnerDal>()), new FakeEmailService());

    private static Present CreatePresent(int id, int quantity, int donorId) => new()
    {
        Id = id,
        Name = $"Present {id}",
        Description = "Test present",
        DonorId = donorId,
        CategoryId = 1,
        ImageUrl = "https://example.com/present.jpg",
        Quantity = quantity,
        Price = 10,
        IsActive = true,
        IsDeleted = false,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = donorId
    };

    private static Lottery CreateLottery(int year) => new()
    {
        Time = new DateOnly(year, 10, 1),
        IsMadeOut = false,
        IsActive = true,
        IsDeleted = false,
        CreatedAt = DateTime.UtcNow
    };

    private static Card CreateCard(int id, Present present, int ownerId) => new()
    {
        Id = id,
        PresentId = present.Id,
        Present = present,
        IsPaid = true,
        IsActive = true,
        IsDeleted = false,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = ownerId
    };

    private sealed class FakeEmailService : IEmailService
    {
    }
}

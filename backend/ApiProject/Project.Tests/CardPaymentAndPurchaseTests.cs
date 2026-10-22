using AutoMapper;
using System.Reflection;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Project;
using Project.Bll;
using Project.Bll.Interfaces;
using Project.Controllers;
using Project.Dal;
using Project.Dto;
using Project.Models;
using Xunit;

namespace Project.Tests;

public class CardPaymentAndPurchaseTests
{
    [Fact]
    public async Task GetMyCards_WhenPaidFilterIsOmitted_ReturnsCardsInBothPaymentStates()
    {
        var service = new CardServiceSpy();
        var controller = CreateController(service, "Development");

        var response = await controller.GetMyCards();

        Assert.IsType<OkObjectResult>(response);
        Assert.Equal(42, service.ListedUserId);
        Assert.Null(service.ListedPaidFilter);
    }

    [Fact]
    public async Task GetCardsByUserAsync_WhenPaymentFilterIsOmitted_ReturnsBothPaymentStatesForOwner()
    {
        await using var context = CreateContext();
        var (user, present) = await SeedPresentAsync(context, quantity: 2, price: 10);
        context.Card.AddRange(
            CreateCard(user.Id, present),
            CreateCard(user.Id, present, isPaid: true),
            CreateCard(user.Id + 1, present));
        await context.SaveChangesAsync();

        var result = await CreateDal(context).GetCardsByUserAsync(user.Id);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count());
        Assert.Contains(result.Data!, card => !card.IsPaid);
        Assert.Contains(result.Data!, card => card.IsPaid);
        Assert.All(result.Data!, card => Assert.Equal(user.Id, card.CreatedBy));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetMyCards_WhenPaidFilterIsProvided_PreservesTheFilter(bool paid)
    {
        var service = new CardServiceSpy();
        var controller = CreateController(service, "Development");

        var response = await controller.GetMyCards(paid);

        Assert.IsType<OkObjectResult>(response);
        Assert.Equal(paid, service.ListedPaidFilter);
    }

    [Fact]
    public async Task DevelopmentPayment_WhenAmountDoesNotMatchUnpaidCards_DoesNotMarkCardsPaid()
    {
        await using var context = CreateContext();
        var (user, present) = await SeedPresentAsync(context, quantity: 1, price: 15);
        var unpaidCard = CreateCard(1, present);
        context.Card.Add(unpaidCard);
        await context.SaveChangesAsync();

        var result = await CreateDal(context).ProcessDevelopmentPaymentAsync(user.Id, 14m);

        Assert.False(result.Success);
        Assert.Contains("does not match", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(unpaidCard.IsPaid);
    }

    [Fact]
    public async Task DevelopmentPayment_VerifiesUnpaidTotalAndOnlyChangesUnpaidCards()
    {
        await using var context = CreateContext();
        var (user, present) = await SeedPresentAsync(context, quantity: 1, price: 15);
        var alreadyPaidCard = CreateCard(1, present, isPaid: true);
        var unpaidCard = CreateCard(1, present);
        var removedCard = CreateCard(1, present);
        removedCard.IsActive = false;
        removedCard.IsDeleted = true;
        var anotherUsersCard = CreateCard(2, present);
        context.Card.AddRange(alreadyPaidCard, unpaidCard, removedCard, anotherUsersCard);
        await context.SaveChangesAsync();

        var dal = CreateDal(context);
        var result = await dal.ProcessDevelopmentPaymentAsync(user.Id, 15m);

        Assert.True(result.Success);
        Assert.True(alreadyPaidCard.IsPaid);
        Assert.True(unpaidCard.IsPaid);
        Assert.False(removedCard.IsPaid);
        Assert.False(anotherUsersCard.IsPaid);
        Assert.Single(result.Data!);

        var listedUnpaidCards = await dal.GetCardsByUserAsync(user.Id, isPaid: false);
        Assert.Single(listedUnpaidCards.Data!);
        Assert.Equal(unpaidCard.Id, listedUnpaidCards.Data!.Single().Id);

        var retry = await dal.ProcessDevelopmentPaymentAsync(user.Id, 15m);
        Assert.True(retry.Success);
        Assert.True(alreadyPaidCard.IsPaid);
        Assert.True(unpaidCard.IsPaid);
    }

    [Fact]
    public async Task AddCard_DoesNotTreatWinnerCountAsTicketSalesLimit()
    {
        await using var context = CreateContext();
        var (user, present) = await SeedPresentAsync(context, quantity: 1, price: 10);
        var dal = CreateDal(context);

        var first = await dal.AddCard(CreateCard(user.Id, present));
        var second = await dal.AddCard(CreateCard(user.Id, present));

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.Equal(2, await context.Card.CountAsync());
    }

    [Fact]
    public async Task AddCard_CreatesRequestedQuantityForAuthenticatedOwnerAndDefaultsToOne()
    {
        await using var context = CreateContext();
        var (user, present) = await SeedPresentAsync(context, quantity: 1, price: 10);
        var mapper = new MapperConfiguration(configuration => configuration.AddProfile<CardProfile>()).CreateMapper();
        var service = new CardService(CreateDal(context), mapper);

        var batchResult = await service.AddCard(
            new CardDto { PresentId = present.Id, Quantity = 3 },
            user.Id);
        var defaultResult = await service.AddCard(new CardDto { PresentId = present.Id }, user.Id);
        var cards = await context.Card.OrderBy(card => card.Id).ToListAsync();

        Assert.True(batchResult.Success);
        Assert.Equal(3, batchResult.Data!.Count());
        Assert.Equal("3 cards created successfully.", batchResult.Message);
        Assert.True(defaultResult.Success);
        Assert.Single(defaultResult.Data!);
        Assert.Equal(4, cards.Count);
        Assert.Equal(cards.Count, cards.Select(card => card.Id).Distinct().Count());
        Assert.All(cards, card =>
        {
            Assert.Equal(user.Id, card.CreatedBy);
            Assert.False(card.IsPaid);
            Assert.True(card.IsActive);
            Assert.False(card.IsDeleted);
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task AddCard_RejectsQuantityOutsideAllowedRange(int quantity)
    {
        await using var context = CreateContext();
        var (user, present) = await SeedPresentAsync(context, quantity: 1, price: 10);
        var mapper = new MapperConfiguration(configuration => configuration.AddProfile<CardProfile>()).CreateMapper();
        var service = new CardService(CreateDal(context), mapper);
        var dto = new CardDto { PresentId = present.Id, Quantity = quantity };
        var validationResults = new List<ValidationResult>();

        var result = await service.AddCard(dto, user.Id);

        Assert.False(Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true));
        Assert.False(result.Success);
        Assert.Empty(await context.Card.ToListAsync());
    }

    [Fact]
    public async Task AddCards_WhenPresentIsUnavailable_DoesNotPersistAnyCards()
    {
        await using var context = CreateContext();
        var (user, present) = await SeedPresentAsync(context, quantity: 1, price: 10);
        present.IsActive = false;
        await context.SaveChangesAsync();
        var cards = Enumerable.Range(0, 3).Select(_ => CreateCard(user.Id, present)).ToArray();

        var result = await CreateDal(context).AddCards(cards);

        Assert.False(result.Success);
        Assert.Empty(await context.Card.ToListAsync());
    }

    [Fact]
    public async Task AddCards_WhenRaffleIsCompleted_DoesNotPersistAnyCards()
    {
        await using var context = CreateContext();
        var (user, present) = await SeedPresentAsync(context, quantity: 1, price: 10);
        var paidCard = CreateCard(user.Id, present, isPaid: true);
        var lottery = new Lottery
        {
            Time = DateOnly.FromDateTime(DateTime.UtcNow),
            IsMadeOut = true,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        context.Card.Add(paidCard);
        context.Lottery.Add(lottery);
        await context.SaveChangesAsync();
        context.Winner.Add(new Winner
        {
            PresentId = present.Id,
            Present = present,
            CardId = paidCard.Id,
            Card = paidCard,
            LotteryId = lottery.Id,
            Lottery = lottery,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id
        });
        await context.SaveChangesAsync();
        var cards = Enumerable.Range(0, 3).Select(_ => CreateCard(user.Id, present)).ToArray();

        var result = await CreateDal(context).AddCards(cards);

        Assert.False(result.Success);
        Assert.Equal(1, await context.Card.CountAsync());
        Assert.All(cards, card => Assert.Equal(0, card.Id));
    }

    [Fact]
    public async Task DevelopmentPaymentEndpoint_IsNotAvailableOutsideDevelopment()
    {
        var service = new CardServiceSpy();
        var controller = CreateController(service, "Production");

        var result = await controller.ProcessDevelopmentPayment(CreateValidDevelopmentPaymentRequest());

        Assert.IsType<NotFoundResult>(result);
        Assert.False(service.PaymentCalled);
    }

    [Fact]
    public async Task DevelopmentPaymentEndpoint_UsesAuthenticatedOwnerAndSubmittedAmountInDevelopment()
    {
        var service = new CardServiceSpy();
        var controller = CreateController(service, "Development");

        var result = await controller.ProcessDevelopmentPayment(CreateValidDevelopmentPaymentRequest());

        Assert.IsType<OkObjectResult>(result);
        Assert.True(service.PaymentCalled);
        Assert.Equal(42, service.PaymentUserId);
        Assert.Equal(15m, service.PaymentAmount);
    }

    [Fact]
    public void AdministrativeWriteEndpoints_RequireBackendAdminAuthorization()
    {
        Assert.Equal("Admin", typeof(CategoryController).GetMethod("CreateCategory")!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>().Single().Roles);
        Assert.Equal("Admin", typeof(CategoryController).GetMethod("UpdateCategory")!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>().Single().Roles);
        Assert.Equal("Admin", typeof(CategoryController).GetMethod("DeleteCategory")!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>().Single().Roles);
        Assert.Equal("Admin", typeof(PresentController).GetMethod("AddPresentAsync")!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>().Single().Roles);
        Assert.Equal("Admin", typeof(PresentController).GetMethod("UpdatePresentAsync")!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>().Single().Roles);
        Assert.Equal("Admin", typeof(PresentController).GetMethod("DeletePresentAsync")!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>().Single().Roles);
        Assert.Equal("Admin", typeof(UserController).GetMethod("AddDonor")!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>().Single().Roles);
        Assert.Equal("Admin", typeof(UserController).GetMethod("AddAdmin")!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>().Single().Roles);
        Assert.Equal("Admin", typeof(RoleController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Single().Roles);
        Assert.Equal("Admin", typeof(UserManagementController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Single().Roles);
        Assert.Equal("Admin", typeof(DonorController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Single().Roles);
        Assert.Equal("Admin", typeof(LotteryController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Single().Roles);
        Assert.Equal("Admin", typeof(WinnerController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Single().Roles);
    }

    private static AppDBContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDBContext(options);
    }

    private static async Task<(User User, Present Present)> SeedPresentAsync(
        AppDBContext context,
        int quantity,
        int price)
    {
        var role = new Role
        {
            Id = 1,
            Name = "User",
            Description = "Standard user role",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var user = new User
        {
            Id = 1,
            Name = "Buyer",
            Email = "buyer@test.com",
            Phone = "123",
            Password = "pw",
            RoleId = role.Id,
            Role = role,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var present = new Present
        {
            Id = 1,
            Name = "Gift",
            Description = "Gift",
            DonorId = user.Id,
            Donor = user,
            CategoryId = 1,
            ImageUrl = "https://example.test/gift.jpg",
            Quantity = quantity,
            Price = price,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id
        };

        context.Role.Add(role);
        context.User.Add(user);
        context.Present.Add(present);
        await context.SaveChangesAsync();
        return (user, present);
    }

    private static Card CreateCard(int userId, Present present, bool isPaid = false) =>
        new()
        {
            PresentId = present.Id,
            Present = present,
            IsPaid = isPaid,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

    private static CardDal CreateDal(AppDBContext context) =>
        new(context, new LoggerFactory().CreateLogger<CardDal>());

    private static CardController CreateController(CardServiceSpy service, string environmentName)
    {
        var responseMapper = new PresentResponseMapper(
            new CategoryDal(
                CreateContext(),
                new LoggerFactory().CreateLogger<CategoryDal>()));
        var controller = new CardController(
            service,
            new TestWebHostEnvironment(environmentName),
            responseMapper);
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "42")],
                authenticationType: "test"))
        };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    private static DevelopmentPaymentRequestDto CreateValidDevelopmentPaymentRequest() =>
        new()
        {
            Amount = 15m,
            CardholderName = "Test Customer",
            CardNumber = "4111111111111111",
            ExpirationMonth = DateTime.UtcNow.Month,
            ExpirationYear = DateTime.UtcNow.Year + 1,
            SecurityCode = "123"
        };

    private sealed class CardServiceSpy : ICardService
    {
        public bool PaymentCalled { get; private set; }
        public int PaymentUserId { get; private set; }
        public decimal PaymentAmount { get; private set; }
        public int ListedUserId { get; private set; }
        public bool? ListedPaidFilter { get; private set; }

        public Task<Result<Card>> AddCard(CardDto cardDto, int userId) => throw new NotImplementedException();
        public Task<Result<Card>> GetCardsByUserAsync(int userId, bool? isPaid = null)
        {
            ListedUserId = userId;
            ListedPaidFilter = isPaid;
            return Task.FromResult(new Result<Card>
            {
                Success = true,
                Data = Array.Empty<Card>()
            });
        }
        public Task<Result<Card>> DeleteCardAsync(int cardId, int userId) => throw new NotImplementedException();
        public Task<object?> GetCardsWithPresentsAsync() => throw new NotImplementedException();

        public Task<Result<Card>> ProcessDevelopmentPaymentAsync(int userId, decimal amount)
        {
            PaymentCalled = true;
            PaymentUserId = userId;
            PaymentAmount = amount;
            return Task.FromResult(new Result<Card>
            {
                Success = true,
                Message = "Development payment processed.",
                Data = Array.Empty<Card>()
            });
        }
    }

    private sealed class TestWebHostEnvironment(string environmentName) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Project.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = environmentName;
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}

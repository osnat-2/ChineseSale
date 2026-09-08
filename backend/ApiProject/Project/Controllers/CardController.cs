using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Attributes;
using Project.Bll.Interfaces;
using Project.Dto;
using Project.Models;

namespace Project.Controllers
{
    [ApiController]
    [Route("api/card/[controller]/")]
    [Authorize]
    public class CardController : ControllerBase
    {
        ICardService _cardService;
        public CardController(ICardService cardService)
        {
            _cardService = cardService;
        }

        ////[Authorize(Roles = "Manager")]
        //[HttpGet("/api/card/getCardsByPresent")]
        //public async Task<Result<Card>> GetCardsWithPresentsAsync()
        //{
        //    return await _cardService.GetCardsWithPresentsAsync();
        //}

        //[RaffleBlock]
        [HttpPost("AddCard")]
        public async Task<Result<Card>> AddCard(CardDto cardDto)
        {
            return await _cardService.AddCard(cardDto);
        }

        ////[Authorize(Roles = "Manager")]
        //[HttpGet("/api/card/getMostExpensiveCards")]
        //public async Task<Result<Card>> GetMostExpensiveCardsAsync()
        //{
        //    return await _cardService.GetMostExpensiveCardsAsync();
        //}

        ////[Authorize(Roles = "Manager")]
        //[HttpGet("/api/card/getAllCardBuyers")]
        //public async Task<Result<User>> GetAllCardBuyersAsync()
        //{
        //    return await _cardService.GetAllCardBuyersAsync();
        //}

        ////[Authorize(Roles = "Manager")]
        //[HttpGet("/api/card/getCardsByQuantity")]
        //public async Task<Result<Card>> GetCardsByQuantityAsync()
        //{
        //    return await _cardService.GetCardsByQuantityAsync();
        //}


        //[HttpDelete("/api/card/deleteCard")]
        //public async Task<Result<Card>> DeleteCardAsync([FromBody] int cardId)
        //{
        //    return await _cardService.DeleteCardAsync(cardId);
        //}

        //[RaffleBlock]
        //[HttpPut("/api/card/payment")]
        //public async Task<Result<Card>> ProcessPaymentForUserAsync(int userId)
        //{
        //    return await _cardService.ProcessPaymentForUserAsync(userId);
        //}

        //[HttpGet("/api/card/getCardsByUser/{userId}")]
        //public async Task<Result<Card>> GetCardsByUserAsync(int userId)
        //{
        //    return await _cardService.GetCardsByUserAsync(userId);
        //}

        ////[Authorize(Roles = "Manager")]
        //[HttpGet("/api/card/getUnpaidCards/{userId}")]
        //public async Task<Result<Card>> GetUnpaidCardsAsync(int userId)
        //{
        //    return await _cardService.GetUnpaidCardsAsync(userId);
        //}
    }
}
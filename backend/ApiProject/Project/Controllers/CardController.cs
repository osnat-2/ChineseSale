using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Attributes;
using Project.Bll.Interfaces;
using Project.Dto;
using Project.Models;
using System.Security.Claims;

namespace Project.Controllers
{
    [ApiController]
    [Route("api/card")]
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
        [HttpPost]
        public async Task<IActionResult> AddCard(CardDto cardDto)
        {
            var result = await _cardService.AddCard(cardDto, GetUserId());
            if (result.Success)
            {
                return Ok(result.Message);
            }
            return BadRequest(result.Message);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyCards([FromQuery] bool? paid = null)
        {
            var cards = await _cardService.GetCardsByUserAsync(GetUserId(), paid);
            if (cards == null)
            {
                return NotFound("No cards found for the user.");
            }
            return Ok(cards);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCard(int id)
        {
            var result = await _cardService.DeleteCardAsync(id, GetUserId());
            if (result.Success)
            {
                return Ok(result.Message);
            }
            return BadRequest(result.Message);
        }

        [HttpPost("payment")]
        public async Task<IActionResult> ProcessPayment()
        {
            var result = await _cardService.ProcessPaymentAsync(GetUserId());
            if (!result.Success)
            {
                return BadRequest("Payment failed. There are still unpaid cards.");
            }
            return Ok("Payment processed successfully.");
        }

        private int GetUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : 0;
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
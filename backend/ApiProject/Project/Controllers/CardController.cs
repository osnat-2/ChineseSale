using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Project.Bll;
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
        private readonly ICardService _cardService;
        private readonly IWebHostEnvironment _environment;
        private readonly PresentResponseMapper _responseMapper;

        public CardController(
            ICardService cardService,
            IWebHostEnvironment environment,
            PresentResponseMapper responseMapper)
        {
            _cardService = cardService;
            _environment = environment;
            _responseMapper = responseMapper;
        }

        //[Authorize(Roles = "Manager")]
        [HttpGet("/api/card/getCardsByPresent")]
        public async Task<IActionResult> GetCardsWithPresentsAsync()
        {
           return Ok(await _cardService.GetCardsWithPresentsAsync());
        }

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

            return Ok(await _responseMapper.MapCardsAsync(cards));
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

        [HttpPost("development-payment")]
        public async Task<IActionResult> ProcessDevelopmentPayment(
            [FromBody] DevelopmentPaymentRequestDto request)
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            var userId = GetUserId();
            if (userId <= 0)
            {
                return Unauthorized();
            }

            var result = await _cardService.ProcessDevelopmentPaymentAsync(userId, request.Amount);
            var response = await _responseMapper.MapCardsAsync(result);
            return response.Success ? Ok(response) : BadRequest(response);
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
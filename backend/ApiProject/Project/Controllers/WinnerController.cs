using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Bll.Interfaces;
using Project.Models;

namespace Project.Controllers
{
    [ApiController]
    [Route("api/winner")]
    [Authorize(Roles = "Admin")]
    public class WinnerController : ControllerBase
    {
        private readonly IWinnerService _winnerService;

        public WinnerController(IWinnerService winnerService)
        {
            _winnerService = winnerService;
        }

        [HttpPost("draw/{presentId:int}")]
        public async Task<Result<Winner>> DrawWinner(int presentId, [FromQuery] int? lotteryId = null)
        {
            return await _winnerService.DrawWinnerForPresentAsync(presentId, lotteryId);
        }
    }
}
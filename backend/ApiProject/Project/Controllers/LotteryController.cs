using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Bll.Interfaces;
using Project.Dto;
using Project.Models;

namespace Project.Controllers
{
    [ApiController]
    [Route("api/lottery")]
    [Authorize(Roles = "Admin")]
    public class LotteryController : ControllerBase
    {
        private readonly ILotteryService _lotteryService;
        private readonly IWinnerService _winnerService;

        public LotteryController(ILotteryService lotteryService, IWinnerService winnerService)
        {
            _lotteryService = lotteryService;
            _winnerService = winnerService;
        }

        [HttpGet]
        public async Task<ActionResult<Result<Lottery>>> GetLotteries()
        {
            var result = await _lotteryService.GetLotteriesAsync();
            return result.Success ? Ok(result) : StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        [HttpPost]
        public async Task<ActionResult<Result<Lottery>>> AddLottery([FromBody] LotteryDto lotteryDto)
        {
            var result = await _lotteryService.AddLotteryAsync(lotteryDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Result<Lottery>>> UpdateLottery(int id, [FromBody] LotteryDto lotteryDto)
        {
            var result = await _lotteryService.UpdateLotteryAsync(id, lotteryDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{id:int}/process")]
        public async Task<ActionResult<Result<Winner>>> ProcessLottery(int id)
        {
            var result = await _winnerService.ProcessLotteryAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("draw/{presentId:int}")]
        public async Task<ActionResult<Result<Winner>>> DrawWinner(
            int presentId,
            [FromQuery] int? lotteryId = null)
        {
            var result = await _winnerService.DrawWinnerForPresentAsync(presentId, lotteryId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
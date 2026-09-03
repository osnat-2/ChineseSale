using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.BLL.Interfaces;
using Project.Models;
using Project.Models.ModelsDTO;

namespace Project.Controllers
{
    [ApiController]
    [Route("api/present")]
    public class PresentController : ControllerBase
    {
        private readonly IPresentService _presentService;

        public PresentController(IPresentService presentService)
        {
            _presentService = presentService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("addPresent")]
        public async Task<Result<Present>> AddPresentAsync([FromBody] PresentDto presentDto)
        {
            return await _presentService.AddPresentAsync(presentDto);
        }

        [HttpGet("getAllPresents")]
        public async Task<Result<Present>> GetAllPresentsAsync([FromQuery] bool onlyActive = true)
        {
            return await _presentService.GetAllPresentsAsync(onlyActive);
        }


        [HttpGet("{id}")]
        public async Task<Result<Present>> GetPresentByIdAsync(int id)
        {
            return await _presentService.GetPresentByIdAsync(id);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("updatePresent/{id}")]
        public async Task<Result<Present>> UpdatePresentAsync(int id, [FromBody] PresentDto presentDto)
        {
            return await _presentService.UpdatePresentAsync(id, presentDto);
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("removePresent/{id}")]
        public async Task<Result<Present>> DeletePresentAsync(int id)
        {
            return await _presentService.DeletePresentAsync(id);
        }


        [HttpGet("getPresentsByName/{name}")]
        public async Task<Result<Present>> GetPresentsByNameAsync(string name)
        {
            return await _presentService.GetPresentsByNameAsync(name);
        }

        [HttpGet("getPresentsByDonor/{donorName}")]
        public async Task<Result<Present>> GetPresentsByDonorNameAsync(string donorName)
        {
            return await _presentService.GetPresentsByDonorNameAsync(donorName);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("getPresentsByBuyerCount/{buyerCount}")]
        public async Task<Result<Present>> GetPresentsByBuyerCountAsync(int buyerCount)
        {
            return await _presentService.GetPresentsByBuyerCountAsync(buyerCount);
        }

        [HttpGet("getPresentsByPrice")]
        public async Task<Result<Present>> GetPresentsByPriceAsync()
        {
            return await _presentService.GetPresentsByPriceAsync();
        }

        [HttpGet("getPresentsByCategory")]
        public async Task<Result<Present>> GetPresentsByCategoryAsync()
        {
            return await _presentService.GetPresentsByCategoryAsync();
        }

        [HttpGet("getAllPresentsWithWinners")]
        public async Task<Result<Present>> GetAllPresentsWithWinnersAsync()
        {
            return await _presentService.GetAllPresentsWithWinnersAsync();
        }
    }
}
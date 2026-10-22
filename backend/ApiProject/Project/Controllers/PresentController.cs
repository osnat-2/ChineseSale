using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Bll;
using Project.Bll.Interfaces;
using Project.Dto;
using Project.Models;
using System.Security.Claims;

namespace Project.Controllers
{
    [ApiController]
    [Route("api/present")]
    public class PresentController : ControllerBase
    {
        private readonly IPresentService _presentService;
        private readonly PresentResponseMapper _responseMapper;
        private readonly PresentImageStorage _imageStorage;

        public PresentController(
            IPresentService presentService,
            PresentResponseMapper responseMapper,
            PresentImageStorage imageStorage)
        {
            _presentService = presentService;
            _responseMapper = responseMapper;
            _imageStorage = imageStorage;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("uploadImage")]
        [RequestSizeLimit(PresentImageStorage.MaximumFileSizeBytes + 64 * 1024)]
        public async Task<ActionResult<Result<PresentImageUploadDto>>> UploadImageAsync(
            IFormFile? image,
            CancellationToken cancellationToken)
        {
            if (image is null)
            {
                return BadRequest(new Result<PresentImageUploadDto>
                {
                    Success = false,
                    Message = "An image file is required."
                });
            }

            var result = await _imageStorage.SaveAsync(image, cancellationToken);
            if (!result.Success)
            {
                var response = new Result<PresentImageUploadDto>
                {
                    Success = false,
                    Message = result.ErrorMessage
                };
                return result.IsTooLarge
                    ? StatusCode(StatusCodes.Status413PayloadTooLarge, response)
                    : BadRequest(response);
            }

            return Ok(new Result<PresentImageUploadDto>
            {
                Success = true,
                Data = [new PresentImageUploadDto { ImageUrl = result.ImageUrl! }]
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("addPresent")]
        public async Task<Result<Present>> AddPresentAsync([FromBody] PresentDto presentDto)
        {
            return await _presentService.AddPresentAsync(presentDto);
        }

        [HttpGet("getAllPresents")]
        public async Task<Result<PresentResponseDto>> GetAllPresentsAsync(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = null)
        {
            var result = await _presentService.GetAllPresentsAsync(onlyActive, search, categoryId, sortBy, sortDirection);
            return await _responseMapper.MapPresentsAsync(result);
        }

        [Authorize(Roles = "Donor")]
        [HttpGet("myPresents")]
        public async Task<IActionResult> GetMyPresentsAsync()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var donorId) || donorId <= 0)
            {
                return Unauthorized();
            }

            var result = await _presentService.GetPresentsByDonorIdAsync(donorId);
            return Ok(await _responseMapper.MapPresentsAsync(result));
        }


        [HttpGet("{id}")]
        public async Task<Result<PresentResponseDto>> GetPresentByIdAsync(int id)
        {
            var result = await _presentService.GetPresentByIdAsync(id);
            return await _responseMapper.MapPresentsAsync(result);
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
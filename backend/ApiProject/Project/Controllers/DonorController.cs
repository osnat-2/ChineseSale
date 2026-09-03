using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.BLL.Interfaces;
using Project.Models;
using Project.Models.ModelsDTO;
using System.Security.Claims;

namespace Project.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/donor")]
    public class DonorController : ControllerBase
    {
        private readonly IDonorService _donorService;

        public DonorController(IDonorService donorService)
        {
            _donorService = donorService;
        }

        [HttpPost("addDonor")]
        public async Task<Result<Donor>> AddDonorAsync([FromBody] DonorDto donorDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return new Result<Donor>
                {
                    Success = false,
                    Message = "Unable to identify the current user.",
                    Data = null
                };
            }

            return await _donorService.AddDonorAsync(donorDto, userId);
        }

        [HttpGet("getAllDonors")]
        public async Task<Result<Donor>> GetAllDonorsAsync([FromQuery] bool onlyActive = true)
        {
            return await _donorService.GetAllDonorsAsync(onlyActive);
        }

        [HttpGet("{id}")]
        public async Task<Result<Donor>> GetDonorByIdAsync(int id)
        {
            return await _donorService.GetDonorByIdAsync(id);
        }

        [HttpGet("getDonorsByEmail/{email}")]
        public async Task<Result<Donor>> GetDonorsByEmailAsync(string email)
        {
            return await _donorService.GetDonorsByEmailAsync(email);
        }

        [HttpGet("getDonorsByName/{name}")]
        public async Task<Result<Donor>> GetDonorsByNameAsync(string name)
        {
            return await _donorService.GetDonorsByNameAsync(name);
        }

        [HttpGet("getDonorsByPresent/{presentName}")] // consider to remove or meybe to change to be by present id
        public async Task<Result<Donor>> GetDonorsByPresentNameAsync(string presentName)
        {
            return await _donorService.GetDonorsByPresentNameAsync(presentName);
        }

        [HttpPut("updateDonor/{id}")]
        public async Task<Result<Donor>> UpdateDonorAsync(int id, [FromBody] DonorDto donorDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return new Result<Donor>
                {
                    Success = false,
                    Message = "Unable to identify the current user.",
                    Data = null
                };
            }

            return await _donorService.UpdateDonorAsync(id, donorDto, userId);
        }

        [HttpDelete("removeDonor/{id}")]
        public async Task<Result<Donor>> RemoveDonorAsync(int id)
        {
            return await _donorService.DeleteDonorAsync(id);
        }
    }
}
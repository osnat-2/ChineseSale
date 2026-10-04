using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Models;

namespace Project.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/donor")]
    public class DonorController : ControllerBase
    {
        [HttpGet("status")]
        public ActionResult<Result<User>> GetDonorStatus()
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new Result<User>
            {
                Success = false,
                Message = "Donor management is intentionally deferred for the current contract baseline. Donor identity is represented by User records with the approved role values.",
                Data = null
            });
        }
    }
}
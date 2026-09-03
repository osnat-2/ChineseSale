using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.BLL.Interfaces;
using Project.Models;
using Project.Models.ModelsDTO;

namespace Project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WinnerController : ControllerBase
    {
        IWinnerService _randomService;
        public WinnerController(IWinnerService randomService)
        {
            _randomService = randomService;
        }
    }
}
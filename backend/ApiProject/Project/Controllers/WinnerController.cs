using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Bll.Interfaces;
using Project.Dto;
using Project.Models;

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
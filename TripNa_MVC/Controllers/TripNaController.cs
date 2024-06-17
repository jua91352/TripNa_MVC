using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripNa_MVC.Models;

namespace TripNa_MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripNaController : ControllerBase
    {
        private TripNaContext _context;

        public TripNaController(TripNaContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Spot>>> GetSpotList()
        {
            return await _context.Spots.ToListAsync();
        }
    }

}

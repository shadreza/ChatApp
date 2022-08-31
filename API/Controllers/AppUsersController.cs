using API.Data;
using API.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppUsersController : ControllerBase
    {
        private readonly DataContext _context;
        public AppUsersController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        // /api/appusers

        // the use of IEnumerable vs List
        // list has many functionalities like sort, search and others but to return a simple list to the users IEnumerable is ample
        public async Task<ActionResult<IEnumerable<AppUser>>> GetAppUsers()
        {
            return await _context.AppUsers.ToListAsync();
        }

        [HttpGet("{id}")]
        // /api/appusers/2
        public async Task<ActionResult<AppUser>> GetAppUser(int id)
        {
            return await _context.AppUsers.FindAsync(id);
        }
    }
}
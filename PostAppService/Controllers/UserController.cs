using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostAppService.Data;
using PostAppService.Models;

namespace PostAppService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly PostAppServiceContext _context;

        public UserController(PostAppServiceContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetPost()
        {
            return await _context.Users.ToListAsync();
        }      
    }
}

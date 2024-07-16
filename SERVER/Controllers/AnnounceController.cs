using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student,Professor")]
    public class AnnounceController(IAnnounce interfaceAnnounce) : Controller
    {
        [HttpGet("GetAnnounce")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAnnounce(string word)
        {
            var result = await interfaceAnnounce.GetAnnounce(word);
            return Ok(result);
        }
    }

}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;

namespace SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student,Professor")]
    public class EmailController(IEmail email) : Controller
    {
        [HttpPost("sendEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> SendEmail([FromBody] EmailDataMethod data)
        {
            var result = await email.SendEmail(data);
            return Ok(result);
        }
    }
}

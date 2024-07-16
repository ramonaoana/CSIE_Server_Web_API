using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student,Professor")]
    public class LeadershipController(ILeadership leadership) : ControllerBase
    {
        [HttpGet("getLeadership")]
        [AllowAnonymous]
        public async Task<IActionResult> getUniversityLeadership()
        {
            var result = await leadership.GetUniveristyLeadership();
            return Ok(result);
        }
    }
}

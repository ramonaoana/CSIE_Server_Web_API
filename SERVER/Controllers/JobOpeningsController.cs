using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student,Professor")]
    public class JobOpeningsController(IJobOpenings jobInterface) : ControllerBase
    {
        [HttpGet("getJobs")]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobs()
        {
            var result = await jobInterface.GetJobOpenings();
            return Ok(result);
        }
    }
}

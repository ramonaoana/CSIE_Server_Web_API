using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class EnrollmentDetailsProfileController(IEnrollmentStudent enrollmentStudent) : ControllerBase
    {
        [HttpGet("getEnrollmentDetailsProfile")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDetailsEnrollmentsStudent(int userId)
        {
            var result = await enrollmentStudent.GetEnrollmentDetails(userId);
            return Ok(result);
        }
    }
}

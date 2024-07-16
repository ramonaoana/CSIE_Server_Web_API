using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student,Professor")]
    public class SpecializationController(ISpecialization server) : ControllerBase
    {
        [HttpGet("getSpecializations")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSpecializations()
        {
            var result = await server.GetSpecializations();

            if (result.Flag && result.Items != null)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, "Eroare in obtinerea listei de specializari.");
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student,Professor")]
    public class ProfessorsController(IProfessors server) : ControllerBase
    {
        [HttpGet("getProfessors")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfessors()
        {
            var result = await server.GetProfessors();

            if (result.Flag && result.Items != null)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, "Eroare in obtinerea listei de profesori.");
            }
        }

        [HttpGet("getProfessorEmailAddress")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfessorEmailAddress(string name)
        {
            var result = await server.GetEmailAddress(name);

            if (result.Flag && result.obj != null)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode(500, "Eroare in obtinerea emailului");
            }
        }
    }
}

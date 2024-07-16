using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController(IUserProfile userProfile) : ControllerBase
    {
        [HttpGet("getProfile")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserProfile(int user)
        {
            var result = await userProfile.GetProfile(user);
            return Ok(result);
        }

        [HttpGet("getNameAndEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> GetNameAndEmail(int userId)
        {
            var result = await userProfile.GetName(userId);
            return Ok(result);
        }

        [HttpGet("getRole")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserRole(int userId)
        {
            var result = await userProfile.CheckUserRole(userId);
            return Ok(result);
        }

        [HttpGet("getCoursesForProfessor")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCoursesForProfessor(int userId)
        {
            var result = await userProfile.GetCoursesForProfessor(userId);
            return Ok(result);
        }
    }
}

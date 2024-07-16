using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthenticationController(IUserAccount accountInterface) : ControllerBase
    {

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> SignInAsync(Login user)
        {
            if (user == null) return BadRequest("Model is empty");
            var result = await accountInterface.SignInAsync(user);
            Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:4200");
            if (result.Flag.Equals(true)) return Ok(result);
            else return BadRequest(result);
        }

        [HttpPost("insertUsers")]
        [AllowAnonymous]
        public async Task<IActionResult> InsertUsers(List<UserAccount> users)
        {
            var result = await accountInterface.InsertUsers(users);
            if (result.Flag.Equals(true)) return Ok(result);
            else return BadRequest(result);
        }


        [HttpPost("insertProfessors")]
        [AllowAnonymous]
        public async Task<IActionResult> InsertProfessors(List<ProfessorAccount> users)
        {
            var result = await accountInterface.InsertProfessors(users);
            if (result.Flag.Equals(true)) return Ok(result);
            else return BadRequest(result);
        }


        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> LogOutAsync(int userId)
        {
            var result = await accountInterface.LogOut(userId);
            return Ok(result);

        }

        [HttpGet("getProfile")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfile(int userId)
        {
            var result = await accountInterface.GetProfile(userId);
            return Ok(result);
        }


        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshTokenAsync(RefreshToken token)
        {
            if (token == null) return BadRequest("Model is empty");
            var result = await accountInterface.RefreshTokenAsync(token);
            return Ok(result);
        }
    }
}

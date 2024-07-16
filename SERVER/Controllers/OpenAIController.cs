using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student, Professor")]
    public class OpenAIController(IOpenAI openAI) : ControllerBase
    {
        [HttpPost("callAI")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMessage(OpenAIRequest openAIRequest)
        {
            var result = await openAI.CompleteSentence(openAIRequest);
            return Ok(result);
        }
    }
}

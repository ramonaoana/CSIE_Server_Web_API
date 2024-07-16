using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student,Professor")]
    public class DocumentController(IDocument document) : ControllerBase
    {
        [HttpGet("getDocuments")]
        [AllowAnonymous]
        public async Task<IActionResult> getDocuments(int type)
        {
            var result = await document.GetDocuments(type);
            return Ok(result);
        }
    }
}

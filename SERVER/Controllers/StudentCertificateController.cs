using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Student")]
    public class StudentCertificateController(IStudentCertificate certificate) : Controller
    {
        [HttpPost("getCertificate")]
        public async Task<IActionResult> GetCertificate([FromBody] DocumentRequest request)
        {
            try
            {
                var result = await certificate.GenerateDocument(request.UserId, request.Reason);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing the request.", error = ex.Message });
            }
        }

        [HttpGet("checkStatus")]
        public async Task<IActionResult> CheckStatus(int studentId)
        {
            try
            {
                var result = await certificate.CheckStatus(studentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing the request.", error = ex.Message });
            }
        }

        [HttpGet("checkStatusUser")]
        public async Task<IActionResult> CheckStatusUser(int userId)
        {
            try
            {
                var result = await certificate.CheckStatusUser(userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing the request.", error = ex.Message });
            }
        }

        [HttpGet("getStudentInfo")]
        public async Task<IActionResult> GetStudentInfo(int user)
        {
            try
            {
                var result = await certificate.GetStudentInfo(user);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing the request.", error = ex.Message });
            }
        }
    }
}


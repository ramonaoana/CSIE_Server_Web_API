using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student,Professor")]
    public class ExamController(IExam exam) : ControllerBase
    {
        [HttpGet("checkCourse")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckCourse(int userId, string course)
        {
            var result = await exam.CheckCourse(userId, course);
            return Ok(result);
        }

        [HttpGet("getExam")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInfoExam(int userId, string course)
        {
            var result = await exam.GetInfoExam(userId, course);
            return Ok(result);
        }
        [HttpGet("getExamGrade")]
        [AllowAnonymous]
        public async Task<IActionResult> GetExamGrade(int examId)
        {
            var result = await exam.GetExamGrade(examId);
            return Ok(result);
        }
    }
}

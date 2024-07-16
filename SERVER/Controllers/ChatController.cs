using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student, Professor")]
    public class ChatController(IChat chatInterface) : ControllerBase
    {
        [HttpPost("sendMessage")]
        [AllowAnonymous]
        public async Task<IActionResult> StartChat([FromBody] ChatMessage message)
        {
            var result = await chatInterface.ClassifyMessage(message);
            return Ok(result);
        }

        [HttpGet("trainModel")]
        [AllowAnonymous]
        public async Task<IActionResult> TrainModel()
        {
            var result = await chatInterface.TrainModelML();
            return Ok(result);
        }

        [HttpPost("classifyMessage")]
        [AllowAnonymous]
        public async Task<IActionResult> ClassifyMessage([FromBody] ChatMessage message)
        {
            var result = await chatInterface.TestClassifyMessage(message);
            return Ok(result);
        }

        [HttpPost("getKeywords")]
        [AllowAnonymous]
        public async Task<IActionResult> GetKeywords([FromBody] ChatMessage message)
        {
            var result = await chatInterface.GetKeywordsNER(message);
            var str = "";
            if (result != null)
            {
                foreach (var entity in result)
                {
                    str += entity.Key + ",";
                }
            }
            else return Ok("Nu s-au identificat cuvintele cheie");
            return Ok(result);
        }

        [HttpPost("queryDocument")]
        [AllowAnonymous]
        public async Task<IActionResult> QueryDocument([FromBody] ChatMessage message, int TypeDocument)
        {
            var result = await chatInterface.QueryPdf(message, TypeDocument);
            if (result.Flag)
            {
                return Ok(result);
            }
            else return BadRequest("Ceva nu a functionat corect.");
        }

        [HttpPost("getStudentCertificate")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStudentCertificate([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getStudentCertificate(message);
            return Ok(result);
        }

        [HttpPost("getEmailProfessor")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEmailProfessor([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getEmailProfessor(message);
            return Ok(result);
        }


        [HttpPost("getInfoScholarship")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInfoScholarship([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getInfoScholarship(message);
            return Ok(result);
        }

        [HttpPost("getInfoExam")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInfoExam([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getInfoExam(message);
            return Ok(result);
        }

        [HttpPost("getExamGrade")]
        [AllowAnonymous]
        public async Task<IActionResult> GetExamGrade([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getExamGrade(message);
            return Ok(result);
        }

        [HttpPost("getAmountScholarship")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAmountScholarship([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getAmountScholarship(message);
            return Ok(result);
        }

        [HttpPost("getInfoCollegeAdmission")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInfoCollegeAdmission([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getInfoCollegeAdmission(message);
            return Ok(result);
        }

        [HttpPost("getScheduleProfessor")]
        [AllowAnonymous]
        public async Task<IActionResult> GetScheduleProfessor([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getScheduleProfessor(message);
            return Ok(result);
        }

        [HttpPost("getInfoMaster")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInfoMaster([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getInfoMaster(message);
            return Ok(result);
        }

        [HttpPost("getInfoStudentCalendar")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInfoStudentCalendar([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getInfoStudentCalendar(message);
            return Ok(result);
        }

        [HttpPost("getInfoBachelorDegree")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInfoBachelorDegree([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getInfoBachelorDegree(message);
            return Ok(result);
        }

        [HttpPost("getInfoMasterDegree")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInfoMasterDegree([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getInfoMasterDegree(message);
            return Ok(result);
        }

        [HttpPost("getInfoCourse")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInfoCourse([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getInfoCourse(message);
            return Ok(result);
        }

        [HttpPost("getInfoBachelorDegreeInternship")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInfoBachelorDegreeInternship([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getInfoBachelorDegreeInternship(message);
            return Ok(result);
        }

        [HttpPost("getInfoMasterInternship")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInfoMasterInternship([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getInfoMasterInternship(message);
            return Ok(result);
        }

        [HttpPost("getSchedule")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSchedule([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getSchedule(message);
            return Ok(result);
        }

        [HttpPost("getScheduleDay")]
        [AllowAnonymous]
        public async Task<IActionResult> GetScheduleDay([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getScheduleDay(message);
            return Ok(result);
        }

        [HttpPost("getScheduleDayAndHour")]
        [AllowAnonymous]
        public async Task<IActionResult> GetScheduleDayAndHour([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getScheduleDayAndHour(message);
            return Ok(result);
        }

        [HttpPost("getCoursesYear")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCoursesYear([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getCoursesYear(message);
            return Ok(result);
        }

        [HttpPost("getInfoScheduleCourse")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInfoScheduleCourse([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getInfoScheduleCourse(message);
            return Ok(result);
        }

        [HttpPost("getJobs")]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobs([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getJobs(message);
            return Ok(result);
        }

        [HttpPost("getGroupsAndSeries")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGroupsAndSeries([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getGroupsAndSeries(message);
            return Ok(result);
        }

        [HttpPost("getGroupsBySpecializationAndYear")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGroupsBySpecializationAndYear([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getGroupsBySpecializationAndYear(message);
            return Ok(result);
        }

        [HttpPost("getGroupsBySpecCycleAndYear")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGroupsBySpecCycleAndYear([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getGroupsBySpecCycleAndYear(message);
            return Ok(result);
        }

        [HttpPost("getGroupsForSeries")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGroupsForSeries([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getGroupsForSeries(message);
            return Ok(result);
        }

        [HttpPost("getSeriesForGroup")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSeriesForGroup([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getSeriesForGroup(message);
            return Ok(result);
        }

        [HttpPost("getSeriesForSpecialization")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSeriesForSpecialization([FromBody] ChatMessage message)
        {
            var result = await chatInterface.getSeriesForSpecialization(message);
            return Ok(result);
        }

        [HttpPost("findAnnounce")]
        [AllowAnonymous]
        public async Task<IActionResult> FindAnnounce([FromBody] ChatMessage message)
        {
            var result = await chatInterface.FindAnnounce(message.Message);
            return Ok(result);
        }





    }
}

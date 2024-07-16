using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;

namespace SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student, Professor")]
    public class ScheduleController(ISchedule schedule) : ControllerBase
    {
        [HttpPost("getScheduleCourse")]
        [AllowAnonymous]
        public async Task<IActionResult> GetScheduleForCourse(int userId, CourseType courseType, [FromBody] string course)
        {
            var result = await schedule.GetScheduleCourse(userId, courseType, course);
            return Ok(result);
        }

        [HttpPost("getScheduleForOneDay")]
        [AllowAnonymous]
        public async Task<IActionResult> GetScheduleForOneDay(int userId, string day)
        {
            var result = await schedule.GetScheduleForOneDay(userId, day);
            return Ok(result);
        }

        [HttpPost("getSchedule")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSchedule(int userId)
        {
            var result = await schedule.GetSchedule(userId);
            return Ok(result);
        }

        [HttpPost("getScheduleDayAndHour")]
        [AllowAnonymous]
        public async Task<IActionResult> GetScheduleDayAndHour(int userId, [FromBody] string day)
        {
            var result = await schedule.GetScheduleDayAndHour(userId, ParamType.TIMP, "Luni");
            return Ok(result);
        }

        [HttpPost("getScheduleForGroup")]
        [AllowAnonymous]
        public async Task<IActionResult> GetScheduleForGroup(int userId, int group)
        {
            var result = await schedule.GetScheduleForGroup(userId, group);
            return Ok(result);
        }


        [HttpPost("getScheduleForSeries")]
        [AllowAnonymous]
        public async Task<IActionResult> GetScheduleForSeries(int userId, string series)
        {
            var result = await schedule.GetScheduleForSeries(userId, series);
            return Ok(result);
        }

        [HttpPost("getScheduleForYearAndSpec")]
        [AllowAnonymous]
        public async Task<IActionResult> GetScheduleForYearAndSpec(int userId, string spec, int year)
        {
            var result = await schedule.GetScheduleForYearAndSpec(userId, spec, year);
            return Ok(result);
        }
    }
}

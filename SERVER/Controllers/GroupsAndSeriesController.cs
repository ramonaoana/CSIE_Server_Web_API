using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace SERVER.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student,Professor")]
    public class GroupsAndSeriesController(IGroupSeries interfaceGroups) : ControllerBase
    {
        [HttpGet("getGroupsForSeries")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGroups(int userId, string series)
        {
            var result = await interfaceGroups.GetGroupsForSeries(userId, series);
            return Ok(result);
        }

        [HttpGet("getSeriesForGroup")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSeriesForGroup(int userId, int groupNr)
        {
            var result = await interfaceGroups.GetSeriesForGroup(userId, groupNr);
            return Ok(result);
        }

        [HttpGet("getSeriesAndGroups")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSeriesAndGroups(int userId)
        {
            var result = await interfaceGroups.GetGroupsAndSeries(userId);
            return Ok(result);
        }

        [HttpGet("getGroupsBySpecializationAndYear")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGroupsBySpecializationAndYear(int userId, string specialization, int year)
        {
            var result = await interfaceGroups.GetGroupsBySpecializationAndYear(userId, specialization, year);
            return Ok(result);
        }

        [HttpGet("getGroupsBySpecCycleAndYear")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGroupsBySpecCycleAndYear(int userId, string specialization, string cycle, int year)
        {
            var result = await interfaceGroups.GetGroupsBySpecCycleAndYear(userId, specialization, cycle, year);
            return Ok(result);
        }

        [HttpGet("getSeriesForSpecialization")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSeriesForSpecialization(int userId, string specialization)
        {
            var result = await interfaceGroups.GetSeriesForSpecialization(userId, specialization);
            return Ok(result);
        }


    }
}

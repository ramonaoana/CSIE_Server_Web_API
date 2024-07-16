using BaseLibrary.DTOs;
using BaseLibrary.Responses;
using ServerLibrary.Helpers;

namespace ServerLibrary.Repositories.Contracts
{
    public interface ISchedule
    {
        Task<ResponseList<InfoSchedule>> GetSchedule(int userId);
        Task<ResponseList<InfoSchedule>> GetScheduleForOneDay(int userId, string day);
        Task<ResponseList<InfoSchedule>> GetScheduleDayAndHour(int userId, ParamType paramType, string day);
        Task<ResponseList<InfoSchedule>> GetScheduleCourse(int userId, CourseType type, string course);
        Task<ResponseList<InfoSchedule>> GetScheduleForGroup(int userId, int groupNr);
        Task<ResponseList<InfoScheduleForSeries>> GetScheduleForSeries(int userId, string series);
        Task<ResponseList<InfoSemesterSchedule>> GetScheduleForYearAndSpec(int userId, string specialization, int year);
    }

}

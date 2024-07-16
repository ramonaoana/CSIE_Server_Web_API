using BaseLibrary.DTOs;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class ScheduleProfessorRepository(AppDbContext appDbContext) : IScheduleProfessor
    {
        public async Task<GeneralResponse> CheckProfessor(string professorName)
        {
            var professor = await appDbContext.Professors
                  .FirstOrDefaultAsync(p => (p.LastName + " " + p.FirstName == professorName)
                  || (p.FirstName + " " + p.LastName) == professorName);
            if (professor == null)
            {
                return new GeneralResponse(false, "Profesorul nu a fost gasit");
            }
            else return new GeneralResponse(true, "Profesorul a fost gasit");
        }

        public async Task<ResponseList<InfoSchedule>> GetSchedule(string professorName)
        {
            var query = from p in appDbContext.Professors
                        join sch in appDbContext.Schedules on p.ProfessorId equals sch.ProfessorId
                        join c in appDbContext.Courses on sch.CourseId equals c.CourseID
                        where (p.LastName + " " + p.FirstName) == professorName ||
                              (p.FirstName + " " + p.LastName) == professorName
                        select new
                        {
                            CourseName = c.CourseName,
                            DayOfWeek = sch.DayOfWeek,
                            Room = sch.Room,
                            ProfessorName = professorName,
                            CourseType = Convert.ToString(sch.Type),
                            Hour = sch.Hour
                        };

            var result = await query.ToListAsync();

            if (result != null)
            {
                DateTime datetimeValue;
                InfoSchedule infoSchedule;
                List<InfoSchedule> listCourses = new List<InfoSchedule>();
                foreach (var item in result)
                {
                    datetimeValue = item.Hour;

                    infoSchedule = new InfoSchedule()
                    {
                        CourseName = item.CourseName,
                        DayOfWeek = item.DayOfWeek,
                        Hour = "" + item.Hour.Hour.ToString("00") + ":" + item.Hour.Minute.ToString("00"),
                        CourseType = item.CourseType,
                        ProfessorName = item.ProfessorName,
                        Room = item.Room.ToString()
                    };
                    listCourses.Add(infoSchedule);
                }
                return new ResponseList<InfoSchedule>(true, listCourses);
            }
            else return new ResponseList<InfoSchedule>(false, null);
        }
    }
}

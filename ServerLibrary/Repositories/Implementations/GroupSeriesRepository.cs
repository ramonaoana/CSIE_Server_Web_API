using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class GroupSeriesRepository(AppDbContext appDbContext) : IGroupSeries
    {
        public async Task<ResponseList<InfoSeriesNameGroups>> GetGroupsAndSeries(int userId)
        {
            var queryAcadYear = await GetInfoAcademicYearForUser(userId);
            if (queryAcadYear.Flag)
            {
                var query = await (from y in appDbContext.AcademicYears
                                   join spec in appDbContext.Specializations on y.SpecializationId equals spec.SpecializationId
                                   join studyCycle in appDbContext.StudyCycles on spec.StudyCycleId equals studyCycle.StudyCycleId
                                   join s in appDbContext.Series on y.AcademicYearId equals s.AcademicYearId
                                   join g in appDbContext.Groups on s.SeriesId equals g.SeriesId
                                   where y.AcademicYearId == queryAcadYear.obj
                                   select new
                                   {
                                       Series = s.SeriesName,
                                       Group = g.GroupNumber
                                   }).ToListAsync();
                if (query != null)
                {
                    var result = query.GroupBy(x => x.Series)
                      .Select(g => new InfoSeriesNameGroups
                      {
                          Series = g.Key,
                          Groups = g.Select(x => x.Group).ToList()
                      }).ToList();
                    return new ResponseList<InfoSeriesNameGroups>(true, result);
                }
                else return new ResponseList<InfoSeriesNameGroups>(false, null);
            }
            else return new ResponseList<InfoSeriesNameGroups>(false, null);
        }

        public async Task<ResponseRequest<int>> GetInfoAcademicYearForUser(int userId)
        {
            var query = await (from u in appDbContext.Users
                               join stud in appDbContext.Students on u.UserId equals stud.UserId
                               join en in appDbContext.EnrollmentStudentsGroups on stud.StudentId equals en.StudentId
                               join g in appDbContext.Groups on en.GroupId equals g.GroupId
                               join s in appDbContext.Series on g.SeriesId equals s.SeriesId
                               join y in appDbContext.AcademicYears on s.AcademicYearId equals y.AcademicYearId
                               where u.UserId == userId && en.Status == 2
                               select new
                               {
                                   AcademicYearId = y.AcademicYearId
                               }).FirstOrDefaultAsync();
            if (query != null)
            {
                return new ResponseRequest<int>(true, query.AcademicYearId);
            }
            else return new ResponseRequest<int>(false, 0);
        }


        public async Task<ResponseList<InfoSeriesNameGroups>> GetGroupsBySpecCycleAndYear(int userId, string specialization, string cycle, int year)
        {
            int currentYear = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            bool checkEndYear = month <= 7;
            bool checkStartYear = month > 7;

            var query = await (from g in appDbContext.Groups
                               join s in appDbContext.Series on g.SeriesId equals s.SeriesId
                               join y in appDbContext.AcademicYears on s.AcademicYearId equals y.AcademicYearId
                               join spec in appDbContext.Specializations on y.SpecializationId equals spec.SpecializationId
                               join studyCycle in appDbContext.StudyCycles on spec.StudyCycleId equals studyCycle.StudyCycleId
                               where (checkStartYear ? y.AcademicStartYear == currentYear : checkEndYear ? y.AcademicEndYear == currentYear : y.AcademicStartYear == currentYear || y.AcademicEndYear == currentYear)
                               && y.Year == year && spec.SpecializationName == specialization && studyCycle.Name == cycle
                               select new
                               {
                                   Series = s.SeriesName,
                                   Group = g.GroupNumber
                               }).ToListAsync();

            if (query != null && query.Any())
            {
                var result = query.GroupBy(x => x.Series)
                  .Select(g => new InfoSeriesNameGroups
                  {
                      Series = g.Key,
                      Groups = g.Select(x => x.Group).ToList()
                  }).ToList();
                return new ResponseList<InfoSeriesNameGroups>(true, result);
            }
            else return new ResponseList<InfoSeriesNameGroups>(false, null);
        }

        public async Task<ResponseList<InfoSeriesNameGroups>> GetGroupsBySpecializationAndYear(int userId, string specialization, int year)
        {
            int currentYear = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            bool checkEndYear = month <= 7;
            bool checkStartYear = month > 7;

            var query = await (from g in appDbContext.Groups
                               join s in appDbContext.Series on g.SeriesId equals s.SeriesId
                               join y in appDbContext.AcademicYears on s.AcademicYearId equals y.AcademicYearId
                               join spec in appDbContext.Specializations on y.SpecializationId equals spec.SpecializationId
                               join cycle in appDbContext.StudyCycles on spec.StudyCycleId equals cycle.StudyCycleId
                               where (checkStartYear ? y.AcademicStartYear == currentYear : checkEndYear ? y.AcademicEndYear == currentYear : y.AcademicStartYear == currentYear || y.AcademicEndYear == currentYear)
                               && y.Year == year && spec.SpecializationName == specialization && cycle.Name == "Licenta"
                               select new
                               {
                                   Series = s.SeriesName,
                                   Group = g.GroupNumber
                               }).ToListAsync();

            if (query != null && query.Any())
            {
                var result = query.GroupBy(x => x.Series)
                  .Select(g => new InfoSeriesNameGroups
                  {
                      Series = g.Key,
                      Groups = g.Select(x => x.Group).ToList()
                  }).ToList();
                return new ResponseList<InfoSeriesNameGroups>(true, result);
            }
            else return new ResponseList<InfoSeriesNameGroups>(false, null);
        }

        public async Task<ResponseRequest<InfoSeriesNameGroups>> GetGroupsForSeries(int userId, string series)
        {
            var query = (from u in appDbContext.Users
                         join stud in appDbContext.Students on u.UserId equals stud.UserId
                         join en in appDbContext.EnrollmentStudentsGroups on stud.StudentId equals en.StudentId
                         join g in appDbContext.Groups on en.GroupId equals g.GroupId
                         join s in appDbContext.Series on g.SeriesId equals s.SeriesId
                         join y in appDbContext.AcademicYears on s.AcademicYearId equals y.AcademicYearId
                         where u.UserId == userId && en.Status == 2 && s.SeriesName == series
                         select new
                         {
                             Series = s.SeriesName,
                             Group = g.GroupNumber
                         }).ToListAsync();
            bool isValid = false;

            if (query != null)
            {
                List<int> groups = new List<int>();
                foreach (var item in query.Result)
                {
                    groups.Add(item.Group);
                }
                isValid = true;
                string seriesInfo = query.Result[0].Series;
                InfoSeriesNameGroups infoSeries = new InfoSeriesNameGroups()
                {
                    Series = seriesInfo,
                    Groups = groups
                };
                return new ResponseRequest<InfoSeriesNameGroups>(isValid, infoSeries);
            }
            else return new ResponseRequest<InfoSeriesNameGroups>(isValid, null);
        }

        public async Task<ResponseRequest<string>> GetSeriesForGroup(int userId, int groupNr)
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            bool checkEndYear = month <= 7;
            bool checkStartYear = month >= 8;

            var query = await (from g in appDbContext.Groups
                               join s in appDbContext.Series on g.SeriesId equals s.SeriesId
                               join y in appDbContext.AcademicYears on s.AcademicYearId equals y.AcademicYearId
                               where (checkStartYear ? y.AcademicStartYear == year : checkEndYear ? y.AcademicEndYear == year : y.AcademicStartYear == year || y.AcademicEndYear == year)
                               && g.GroupNumber == groupNr
                               select new
                               {
                                   Series = s
                               }).FirstOrDefaultAsync();

            if (query != null)
            {
                return new ResponseRequest<string>(true, query.Series.SeriesName);
            }
            else return new ResponseRequest<string>(false, null);
        }

        public async Task<ResponseList<InfoSeriesSpecialization>> GetSeriesForSpecialization(int userId, string specialization)
        {
            int currentYear = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            bool checkEndYear = month <= 7;
            bool checkStartYear = month > 7;

            var query = await (from g in appDbContext.Groups
                               join s in appDbContext.Series on g.SeriesId equals s.SeriesId
                               join y in appDbContext.AcademicYears on s.AcademicYearId equals y.AcademicYearId
                               join spec in appDbContext.Specializations on y.SpecializationId equals spec.SpecializationId
                               join cycle in appDbContext.StudyCycles on spec.StudyCycleId equals cycle.StudyCycleId
                               where (checkStartYear ? y.AcademicStartYear == currentYear : checkEndYear ? y.AcademicEndYear == currentYear : y.AcademicStartYear == currentYear || y.AcademicEndYear == currentYear)
                               && spec.SpecializationName == specialization
                               select new
                               {
                                   Year = y.Year,
                                   Series = s.SeriesName,
                                   Group = g.GroupNumber
                               }).ToListAsync();

            if (query != null && query.Any())
            {
                var result = query.GroupBy(x => x.Year)
                  .Select(g => new InfoSeriesSpecialization
                  {
                      Year = g.Key,
                      Series = g.GroupBy(x => x.Series)
                                     .Select(sg => new InfoSeriesNameGroups
                                     {
                                         Series = sg.Key,
                                         Groups = sg.Select(x => x.Group).ToList()
                                     }).ToList()
                  }).ToList();

                return new ResponseList<InfoSeriesSpecialization>(true, result);
            }
            else return new ResponseList<InfoSeriesSpecialization>(false, null);
        }
    }
}

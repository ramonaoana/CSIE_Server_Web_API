using BaseLibrary.DTOs;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class ScheduleRepository(AppDbContext appDbContext) : ISchedule
    {
        public async Task<ResponseList<InfoSchedule>> GetSchedule(int userId)
        {
            StudentCertificateRepository repository = new StudentCertificateRepository(appDbContext);
            var response = await repository.CheckStatusUser(userId);
            if (response.Flag)
            {
                var query = from u in appDbContext.Users
                            join stud in appDbContext.Students on u.UserId equals stud.UserId
                            join en in appDbContext.EnrollmentStudentsGroups on stud.StudentId equals en.StudentId
                            join enSem in appDbContext.EnrollmentSemesters on en.EnrollmentStudentGroupId equals enSem.EnrollmentStudentGroupId
                            join g in appDbContext.Groups on en.GroupId equals g.GroupId
                            join sch in appDbContext.Schedules on enSem.SemesterId equals sch.SemesterId
                            join c in appDbContext.Courses on sch.CourseId equals c.CourseID
                            join p in appDbContext.Professors on sch.ProfessorId equals p.ProfessorId
                            where u.UserId == userId && en.Status == 2 && enSem.Status == 2
                            select new
                            {
                                CourseName = c.CourseName,
                                DayOfWeek = sch.DayOfWeek,
                                Room = sch.Room,
                                ProfessorName = p.FirstName + " " + p.LastName,
                                CourseType = Convert.ToString(sch.Type),
                                Hour = sch.Hour
                            };

                var result = await query.ToListAsync();
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
                return new ResponseList<InfoSchedule>(response.Flag, listCourses);
            }
            return new ResponseList<InfoSchedule>(response.Flag, null);
        }


        public async Task<ResponseList<InfoSchedule>> GetScheduleForOneDay(int userId, string day)
        {
            StudentCertificateRepository repository = new StudentCertificateRepository(appDbContext);
            var response = await repository.CheckStatusUser(userId);
            if (response.Flag)
            {
                var query = from u in appDbContext.Users
                            join stud in appDbContext.Students on u.UserId equals stud.UserId
                            join en in appDbContext.EnrollmentStudentsGroups on stud.StudentId equals en.StudentId
                            join enSem in appDbContext.EnrollmentSemesters on en.EnrollmentStudentGroupId equals enSem.EnrollmentStudentGroupId
                            join g in appDbContext.Groups on en.GroupId equals g.GroupId
                            join sch in appDbContext.Schedules on enSem.SemesterId equals sch.SemesterId
                            join c in appDbContext.Courses on sch.CourseId equals c.CourseID
                            join p in appDbContext.Professors on sch.ProfessorId equals p.ProfessorId
                            where u.UserId == userId && en.Status == 2 && enSem.Status == 2 && sch.DayOfWeek.ToUpper() == day.ToUpper()
                            select new
                            {
                                CourseName = c.CourseName,
                                DayOfWeek = sch.DayOfWeek,
                                Room = sch.Room,
                                ProfessorName = p.FirstName + " " + p.LastName,
                                CourseType = Convert.ToString(sch.Type),
                                Hour = sch.Hour
                            };

                var result = await query.ToListAsync();
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
                return new ResponseList<InfoSchedule>(response.Flag, listCourses);
            }
            return new ResponseList<InfoSchedule>(response.Flag, null);
        }

        public async Task<ResponseList<InfoSchedule>> GetScheduleForGroup(int userId, int groupNr)
        {
            StudentCertificateRepository repository = new StudentCertificateRepository(appDbContext);
            var response = await repository.CheckStatusUser(userId);
            if (response.Flag)
            {
                var query = from u in appDbContext.Users
                            join stud in appDbContext.Students on u.UserId equals stud.UserId
                            join en in appDbContext.EnrollmentStudentsGroups on stud.StudentId equals en.StudentId
                            join enSem in appDbContext.EnrollmentSemesters on en.EnrollmentStudentGroupId equals enSem.EnrollmentStudentGroupId
                            join g in appDbContext.Groups on en.GroupId equals g.GroupId
                            join sch in appDbContext.Schedules on enSem.SemesterId equals sch.SemesterId
                            join c in appDbContext.Courses on sch.CourseId equals c.CourseID
                            join p in appDbContext.Professors on sch.ProfessorId equals p.ProfessorId
                            where u.UserId == userId && en.Status == 2 && enSem.Status == 2 && g.GroupNumber == groupNr
                            select new
                            {

                                CourseName = c.CourseName,
                                DayOfWeek = sch.DayOfWeek,
                                Room = sch.Room,
                                ProfessorName = p.FirstName + " " + p.LastName,
                                CourseType = Convert.ToString(sch.Type),
                                Hour = sch.Hour
                            };

                var result = await query.ToListAsync();
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
                return new ResponseList<InfoSchedule>(response.Flag, listCourses);
            }
            return new ResponseList<InfoSchedule>(response.Flag, null);
        }

        public async Task<ResponseList<InfoScheduleForSeries>> GetScheduleForSeries(int userId, string series)
        {
            int currentYear = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            bool checkEndYear = month <= 7;
            bool checkStartYear = month > 7;
            StudentCertificateRepository repository = new StudentCertificateRepository(appDbContext);
            var response = await repository.CheckStatusUser(userId);
            if (response.Flag)
            {
                var query = from u in appDbContext.Users
                            join stud in appDbContext.Students on u.UserId equals stud.UserId
                            join en in appDbContext.EnrollmentStudentsGroups on stud.StudentId equals en.StudentId
                            join enSem in appDbContext.EnrollmentSemesters on en.EnrollmentStudentGroupId equals enSem.EnrollmentStudentGroupId
                            join g in appDbContext.Groups on en.GroupId equals g.GroupId
                            join s in appDbContext.Series on g.SeriesId equals s.SeriesId
                            join sch in appDbContext.Schedules on enSem.SemesterId equals sch.SemesterId
                            join c in appDbContext.Courses on sch.CourseId equals c.CourseID
                            join p in appDbContext.Professors on sch.ProfessorId equals p.ProfessorId
                            join ay in appDbContext.AcademicYears on s.AcademicYearId equals ay.AcademicYearId
                            where (checkStartYear ? ay.AcademicStartYear == currentYear : checkEndYear ? ay.AcademicEndYear == currentYear : ay.AcademicStartYear == currentYear || ay.AcademicEndYear == currentYear)
                            && u.UserId == userId && en.Status == 2 && s.SeriesName == series && enSem.Status == 2
                            select new
                            {
                                CourseName = c.CourseName,
                                DayOfWeek = sch.DayOfWeek,
                                Room = sch.Room,
                                ProfessorName = p.FirstName + " " + p.LastName,
                                CourseType = Convert.ToString(sch.Type),
                                Hour = sch.Hour,
                                GroupNumber = g.GroupNumber
                            };
                List<InfoScheduleForSeries> groupedResult = null;
                var result = await query.ToListAsync();
                if (result != null)
                {
                    groupedResult = result.GroupBy(r => new
                    {
                        r.CourseName,
                        r.DayOfWeek,
                        r.Room,
                        r.ProfessorName,
                        r.CourseType,
                        r.Hour
                    }).Select(g => new InfoScheduleForSeries
                    {
                        CourseName = g.Key.CourseName,
                        DayOfWeek = g.Key.DayOfWeek,
                        Hour = g.Key.Hour.ToString("HH:mm"),
                        CourseType = g.Key.CourseType,
                        ProfessorName = g.Key.ProfessorName,
                        Room = g.Key.Room.ToString(),
                        Groups = g.Select(grp => grp.GroupNumber).Distinct().ToList()
                    }).ToList();
                }
                return new ResponseList<InfoScheduleForSeries>(response.Flag, groupedResult);
            }
            return new ResponseList<InfoScheduleForSeries>(response.Flag, null);
        }

        public async Task<ResponseList<InfoSemesterSchedule>> GetScheduleForYearAndSpec(int userId, string specialization, int year)
        {
            int currentYear = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            bool checkEndYear = month <= 7;
            bool checkStartYear = month > 7;
            StudentCertificateRepository repository = new StudentCertificateRepository(appDbContext);
            var response = await repository.CheckStatusUser(userId);
            if (response.Flag)
            {
                var query = from ay in appDbContext.AcademicYears
                            join sem in appDbContext.Semesters on ay.AcademicYearId equals sem.AcademicYearId
                            join sch in appDbContext.Schedules on sem.SemesterId equals sch.SemesterId
                            join schGroup in appDbContext.ScheduleGroups on sch.ScheduleId equals schGroup.ScheduleId
                            join g in appDbContext.Groups on schGroup.GroupId equals g.GroupId
                            join s in appDbContext.Series on g.SeriesId equals s.SeriesId
                            join c in appDbContext.Courses on sch.CourseId equals c.CourseID
                            join p in appDbContext.Professors on sch.ProfessorId equals p.ProfessorId
                            join spec in appDbContext.Specializations on ay.SpecializationId equals spec.SpecializationId
                            where (checkStartYear ? ay.AcademicStartYear == currentYear : checkEndYear ? ay.AcademicEndYear == currentYear : ay.AcademicStartYear == currentYear || ay.AcademicEndYear == currentYear)
                            && ay.Year == year && spec.SpecializationName == specialization
                            select new
                            {
                                CourseName = c.CourseName,
                                DayOfWeek = sch.DayOfWeek,
                                Room = sch.Room,
                                ProfessorName = p.FirstName + " " + p.LastName,
                                CourseType = Convert.ToString(sch.Type),
                                Hour = sch.Hour,
                                Semester = sem.SemesterNumber,
                                SeriesName = s.SeriesName,
                                GroupNumber = g.GroupNumber
                            };

                var result = await query.ToListAsync();

                var groupedBySemester = result.GroupBy(r => r.Semester).Select(g => new InfoSemesterSchedule
                {
                    Semester = g.Key,
                    Series = g.GroupBy(s => s.SeriesName).Select(sg => new InfoSeriesSchedule
                    {
                        SeriesName = sg.Key,
                        Schedules = sg.Select(s => new InfoScheduleForSeries
                        {
                            CourseName = s.CourseName,
                            DayOfWeek = s.DayOfWeek,
                            Hour = s.Hour.ToString("HH:mm"),
                            CourseType = s.CourseType,
                            ProfessorName = s.ProfessorName,
                            Room = s.Room.ToString(),
                            Groups = sg.Select(grp => grp.GroupNumber).Distinct().ToList()
                        }).ToList()
                    }).ToList()
                }).ToList();

                return new ResponseList<InfoSemesterSchedule>(response.Flag, groupedBySemester);
            }
            return new ResponseList<InfoSemesterSchedule>(response.Flag, null);
        }

        public async Task<ResponseList<InfoSchedule>> GetScheduleCourse(int userId, CourseType type, string course)
        {
            string typeCourse;
            if (type == CourseType.Curs)
            {
                typeCourse = "C";
            }
            else typeCourse = "S";
            var result = await (from u in appDbContext.Users
                                join stud in appDbContext.Students on u.UserId equals stud.UserId
                                join en in appDbContext.EnrollmentStudentsGroups on stud.StudentId equals en.StudentId
                                join enSem in appDbContext.EnrollmentSemesters on en.EnrollmentStudentGroupId equals enSem.EnrollmentStudentGroupId
                                join g in appDbContext.Groups on en.GroupId equals g.GroupId
                                join sch in appDbContext.Schedules on enSem.SemesterId equals sch.SemesterId
                                join c in appDbContext.Courses on sch.CourseId equals c.CourseID
                                join p in appDbContext.Professors on sch.ProfessorId equals p.ProfessorId
                                where u.UserId == userId && en.Status == 2 && c.CourseName == course && sch.Type == typeCourse && enSem.Status == 2
                                select new
                                {

                                    CourseName = c.CourseName,
                                    DayOfWeek = sch.DayOfWeek,
                                    Room = sch.Room,
                                    ProfessorName = p.FirstName + " " + p.LastName,
                                    CourseType = Convert.ToString(sch.Type),
                                    Hour = sch.Hour
                                }).FirstOrDefaultAsync();
            string message = "";
            DateTime datetimeValue;
            InfoSchedule infoSchedule;
            List<InfoSchedule> listCourses = new List<InfoSchedule>();
            bool isCorrect = false;
            if (result != null)
            {
                isCorrect = true;
                datetimeValue = result.Hour;
                infoSchedule = new InfoSchedule()
                {
                    CourseName = result.CourseName,
                    DayOfWeek = result.DayOfWeek,
                    Hour = "" + result.Hour.Hour.ToString("00") + ":" + result.Hour.Minute.ToString("00"),
                    CourseType = result.CourseType,
                    ProfessorName = result.ProfessorName,
                    Room = result.Room.ToString()
                };
                listCourses.Add(infoSchedule);

                return new ResponseList<InfoSchedule>(isCorrect, listCourses);
            }
            else return new ResponseList<InfoSchedule>(isCorrect, null);
        }

        public async Task<ResponseList<InfoSchedule>> GetScheduleDayAndHour(int userId, ParamType paramType, string day)
        {
            var result = await (from u in appDbContext.Users
                                join stud in appDbContext.Students on u.UserId equals stud.UserId
                                join en in appDbContext.EnrollmentStudentsGroups on stud.StudentId equals en.StudentId
                                join enSem in appDbContext.EnrollmentSemesters on en.EnrollmentStudentGroupId equals enSem.EnrollmentStudentGroupId
                                join g in appDbContext.Groups on en.GroupId equals g.GroupId
                                join sch in appDbContext.Schedules on enSem.SemesterId equals sch.SemesterId
                                join c in appDbContext.Courses on sch.CourseId equals c.CourseID
                                join p in appDbContext.Professors on sch.ProfessorId equals p.ProfessorId
                                where u.UserId == userId && en.Status == 2 && sch.DayOfWeek == day && enSem.Status == 2
                                orderby sch.Hour
                                select new
                                {
                                    CourseName = c.CourseName,
                                    DayOfWeek = sch.DayOfWeek,
                                    Room = sch.Room,
                                    ProfessorName = p.FirstName + " " + p.LastName,
                                    CourseType = Convert.ToString(sch.Type),
                                    Hour = sch.Hour
                                }).FirstOrDefaultAsync();

            string message = "";
            DateTime datetimeValue;
            InfoSchedule infoSchedule;
            List<InfoSchedule> listCourses = new List<InfoSchedule>();
            bool isCorrect = false;

            if (result != null)
            {
                isCorrect = true;
                var item = result;
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
                return new ResponseList<InfoSchedule>(isCorrect, listCourses);
            }
            else return new ResponseList<InfoSchedule>(isCorrect, null);
        }
    }
}


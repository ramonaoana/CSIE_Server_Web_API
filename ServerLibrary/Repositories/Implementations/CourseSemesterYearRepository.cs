using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class CourseSemesterYearRepository(AppDbContext appDbContext) : ICourseSemesterYear
    {
        public async Task<ResponseRequest<Semester>> getSemester(int userId)
        {
            var query = from u in appDbContext.Users
                        join stud in appDbContext.Students on u.UserId equals stud.UserId
                        join en in appDbContext.EnrollmentStudentsGroups on stud.StudentId equals en.StudentId
                        join enrollmentSem in appDbContext.EnrollmentSemesters on en.EnrollmentStudentGroupId equals enrollmentSem.EnrollmentStudentGroupId
                        join sem in appDbContext.Semesters on enrollmentSem.SemesterId equals sem.SemesterId
                        where u.UserId == userId && en.Status == 2 && enrollmentSem.Status == 2
                        select new
                        {
                            Semester = sem
                        };
            var result = await query.FirstOrDefaultAsync();
            if (result != null)
            {
                return new ResponseRequest<Semester>(true, result.Semester);
            }
            else return new ResponseRequest<Semester>(false, null);
        }

        public async Task<ResponseRequest<AcademicYear>> getAcademicYear(int userId)
        {
            var query = from u in appDbContext.Users
                        join stud in appDbContext.Students on u.UserId equals stud.UserId
                        join en in appDbContext.EnrollmentStudentsGroups on stud.StudentId equals en.StudentId
                        join g in appDbContext.Groups on en.GroupId equals g.GroupId
                        join s in appDbContext.Series on g.SeriesId equals s.SeriesId
                        join y in appDbContext.AcademicYears on s.AcademicYearId equals y.AcademicYearId
                        where u.UserId == userId && en.Status == 2
                        select new
                        {
                            Year = y
                        };
            var result = await query.FirstOrDefaultAsync();
            if (result != null)
            {
                return new ResponseRequest<AcademicYear>(true, result.Year);
            }
            else return new ResponseRequest<AcademicYear>(false, null);
        }

        public async Task<ResponseRequest<AcademicYear>> getAcademicYearByName(int userId, int number)
        {
            var resultAcademicYear = await getAcademicYear(userId);
            AcademicYear actualAcademicYear = null;
            if (resultAcademicYear != null)
            {
                actualAcademicYear = resultAcademicYear.obj;
                var query = from y in appDbContext.AcademicYears
                            join s in appDbContext.Semesters on y.AcademicYearId equals s.AcademicYearId
                            join cs in appDbContext.CoursesSemesters on s.SemesterId equals cs.SemesterId
                            join c in appDbContext.Courses on cs.CourseId equals c.CourseID
                            join spec in appDbContext.Specializations on y.SpecializationId equals spec.SpecializationId
                            join cycle in appDbContext.StudyCycles on spec.StudyCycleId equals cycle.StudyCycleId
                            where y.Year == number && y.AcademicStartYear == actualAcademicYear.AcademicStartYear && y.AcademicEndYear == actualAcademicYear.AcademicEndYear
                            && y.SpecializationId == actualAcademicYear.SpecializationId && y.EducationFormId == actualAcademicYear.EducationFormId
                            select new
                            {
                                Year = y
                            };
                var result = await query.FirstOrDefaultAsync();
                if (result != null)
                {
                    return new ResponseRequest<AcademicYear>(true, result.Year);
                }
                else return new ResponseRequest<AcademicYear>(false, null);
            }
            else return new ResponseRequest<AcademicYear>(false, null);
        }

        public async Task<ResponseList<Course>> getSemesterCourses(int userId, int semester)
        {
            //var year = await getAcademicYear(userId);
            //var query = from u in appDbContext.Users
            //            join stud in appDbContext.Students on u.UserId equals stud.UserId
            //            join en in appDbContext.EnrollmentStudentsGroups on stud.StudentId equals en.StudentId
            //            join g in appDbContext.Groups on en.GroupId equals g.GroupId
            //            join sg in appDbContext.ScheduleGroups on g.GroupId equals sg.GroupId
            //            join sch in appDbContext.Schedules on sg.ScheduleId equals sch.ScheduleId
            //            join c in appDbContext.Courses on sch.CourseId equals c.CourseID
            //            join p in appDbContext.Professors on sch.ProfessorId equals p.ProfessorId
            //            where u.UserId == userId && en.Status == 2
            //            select new
            //            {
            //                CourseName = c,
            //            };
            return null;
        }

        public async Task<ResponseList<CoursesYearInfo>> getYearCourses(int userId, int yearId)
        {
            var queryYear = await getAcademicYearByName(userId, yearId);
            if (queryYear != null)
            {
                var queryCourses = await (from y in appDbContext.AcademicYears
                                          join sem in appDbContext.Semesters on y.AcademicYearId equals sem.AcademicYearId
                                          join cs in appDbContext.CoursesSemesters on sem.SemesterId equals cs.SemesterId
                                          join c in appDbContext.Courses on cs.CourseId equals c.CourseID
                                          where y.AcademicYearId == queryYear.obj.AcademicYearId
                                          group c by sem into g
                                          select new
                                          {
                                              Semester = g.Key.SemesterNumber,
                                              Courses = g.Select(course => new
                                              {
                                                  course.CourseName,
                                                  course.Assessment,
                                                  course.Credits,
                                                  course.Type
                                              }).ToList()
                                          }).ToListAsync();

                if (queryCourses != null)
                {
                    List<CoursesYearInfo> courses = new List<CoursesYearInfo>();
                    foreach (var item in queryCourses)
                    {
                        List<CourseInfo> coursesInfo = new List<CourseInfo>();
                        foreach (var course in item.Courses)
                        {
                            CourseInfo cInfo = new CourseInfo();
                            cInfo.CourseName = course.CourseName;
                            cInfo.Assessment = course.Assessment;
                            cInfo.Credits = course.Credits;
                            cInfo.Type = course.Type;
                            coursesInfo.Add(cInfo);
                        }
                        CoursesYearInfo courseInfo = new CoursesYearInfo
                        {
                            Semester = item.Semester,
                            Courses = coursesInfo
                        };
                        courses.Add(courseInfo);
                    }
                    return new ResponseList<CoursesYearInfo>(true, courses);
                }
                else return new ResponseList<CoursesYearInfo>(false, null);
            }
            else return new ResponseList<CoursesYearInfo>(false, null);
        }

        public async Task<ResponseList<CoursesYearInfo>> getYearSemesterCourses(int userId, int year, int semester)
        {
            var queryYear = await getAcademicYearByName(userId, year);
            if (queryYear != null)
            {
                var queryCourses = await (from y in appDbContext.AcademicYears
                                          join sem in appDbContext.Semesters on y.AcademicYearId equals sem.AcademicYearId
                                          join cs in appDbContext.CoursesSemesters on sem.SemesterId equals cs.SemesterId
                                          join c in appDbContext.Courses on cs.CourseId equals c.CourseID
                                          where y.AcademicYearId == queryYear.obj.AcademicYearId && sem.SemesterNumber == semester
                                          group c by sem into g
                                          select new
                                          {
                                              Semester = g.Key.SemesterNumber,
                                              Courses = g.Select(course => new
                                              {
                                                  course.CourseName,
                                                  course.Assessment,
                                                  course.Credits,
                                                  course.Type
                                              }).ToList()
                                          }).ToListAsync();

                if (queryCourses != null)
                {
                    List<CoursesYearInfo> courses = new List<CoursesYearInfo>();
                    foreach (var item in queryCourses)
                    {
                        List<CourseInfo> coursesInfo = new List<CourseInfo>();
                        foreach (var course in item.Courses)
                        {
                            CourseInfo cInfo = new CourseInfo();
                            cInfo.CourseName = course.CourseName;
                            cInfo.Assessment = course.Assessment;
                            cInfo.Credits = course.Credits;
                            cInfo.Type = course.Type;
                            coursesInfo.Add(cInfo);
                        }
                        CoursesYearInfo courseInfo = new CoursesYearInfo
                        {
                            Semester = item.Semester,
                            Courses = coursesInfo
                        };
                        courses.Add(courseInfo);
                    }
                    return new ResponseList<CoursesYearInfo>(true, courses);
                }
                else return new ResponseList<CoursesYearInfo>(false, null);
            }
            else return new ResponseList<CoursesYearInfo>(false, null);
        }
    }
}

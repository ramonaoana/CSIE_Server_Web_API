using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class ExamRepository(AppDbContext appDbContext) : IExam
    {
        public async Task<ResponseRequest<Schedule>> CheckCourse(int userId, string course)
        {
            Schedule resultCourse = new Schedule();
            Boolean wasFound = false;
            var result = await (from u in appDbContext.Users
                                join stud in appDbContext.Students on u.UserId equals stud.UserId
                                join en in appDbContext.EnrollmentStudentsGroups on stud.StudentId equals en.StudentId
                                join enSem in appDbContext.EnrollmentSemesters on en.EnrollmentStudentGroupId equals enSem.EnrollmentStudentGroupId
                                join g in appDbContext.Groups on en.GroupId equals g.GroupId
                                join sch in appDbContext.Schedules on enSem.SemesterId equals sch.SemesterId
                                join c in appDbContext.Courses on sch.CourseId equals c.CourseID
                                join p in appDbContext.Professors on sch.ProfessorId equals p.ProfessorId
                                where u.UserId == userId && en.Status == 2 && enSem.Status == 2 && c.CourseName == course && sch.Type == "C"
                                select new
                                {
                                    Schedule = sch
                                }).FirstOrDefaultAsync();

            if (result == null)
            {
                var queryEnrollments = await (from u in appDbContext.Users
                                              join stud in appDbContext.Students on u.UserId equals stud.UserId
                                              join en in appDbContext.Enrollments on stud.StudentId equals en.StudentId
                                              join sch in appDbContext.Schedules on en.ScheduleId equals sch.ScheduleId
                                              join c in appDbContext.Courses on sch.CourseId equals c.CourseID
                                              where u.UserId == userId && c.CourseName == course && sch.Type == "C"
                                              select new
                                              {
                                                  Schedule = sch
                                              }).FirstOrDefaultAsync();

                if (queryEnrollments != null)
                {
                    wasFound = true;
                    resultCourse = queryEnrollments.Schedule;
                }
            }
            else
            {
                wasFound = true;
                resultCourse = result.Schedule;
            }

            if (wasFound)
            {
                return new ResponseRequest<Schedule>(wasFound, resultCourse);
            }
            else return new ResponseRequest<Schedule>(wasFound, null);
        }

        public async Task<ResponseRequest<ExamGrade>> GetExamGrade(int examId)
        {
            var query = await (from e in appDbContext.Exams
                               join g in appDbContext.ExamGrades on e.ExamId equals g.ExamId
                               where e.ExamId == examId
                               select new
                               {
                                   ExamGrade = g
                               }).FirstOrDefaultAsync();
            if (query != null)
            {
                return new ResponseRequest<ExamGrade>(true, query.ExamGrade);
            }
            else return new ResponseRequest<ExamGrade>(false, null);
        }

        public async Task<ResponseRequest<Exam>> GetInfoExam(int userId, string course)
        {
            var querySchedule = await CheckCourse(userId, course);
            bool wasFound = false;
            Exam exam = new Exam();
            if (querySchedule.obj != null)
            {
                var id = querySchedule.obj.ScheduleId;
                var query = await (from s in appDbContext.Schedules
                                   join e in appDbContext.Exams on s.ScheduleId equals e.ScheduleId
                                   where s.ScheduleId == id
                                   select new
                                   {
                                       Exam = e
                                   }).FirstOrDefaultAsync();

                if (query != null)
                {
                    exam = query.Exam;
                    wasFound = true;
                }
            }
            else wasFound = false;

            if (wasFound)
            {
                return new ResponseRequest<Exam>(wasFound, exam);
            }
            else return new ResponseRequest<Exam>(wasFound, null);
        }
    }
}

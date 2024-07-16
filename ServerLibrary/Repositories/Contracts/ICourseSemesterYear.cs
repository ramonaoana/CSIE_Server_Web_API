using BaseLibrary.Entities;
using BaseLibrary.Responses;
using ServerLibrary.Helpers;

namespace ServerLibrary.Repositories.Contracts
{
    public interface ICourseSemesterYear
    {
        Task<ResponseList<Course>> getSemesterCourses(int userId, int semester);
        Task<ResponseRequest<AcademicYear>> getAcademicYear(int userId);
        Task<ResponseRequest<Semester>> getSemester(int userId);
        Task<ResponseList<CoursesYearInfo>> getYearCourses(int userId, int yearId);
        Task<ResponseRequest<AcademicYear>> getAcademicYearByName(int userId, int number);
        Task<ResponseList<CoursesYearInfo>> getYearSemesterCourses(int userId, int year, int semester);

    }
}

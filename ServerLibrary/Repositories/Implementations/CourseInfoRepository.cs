using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class CourseInfoRepository(AppDbContext appDbContext) : ICourseInfo
    {
        public async Task<ResponseRequest<Course>> GetCourse(string name)
        {
            bool flag = false;
            Course course = new Course();
            try
            {
                course = await appDbContext.Courses.Where(c => c.CourseName == name).FirstOrDefaultAsync();
                flag = true;
            }
            catch (Exception ex)
            {
            }
            return new ResponseRequest<Course>(flag, course);
        }
    }
}

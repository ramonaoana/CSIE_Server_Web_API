using BaseLibrary.Entities;
using BaseLibrary.Responses;

namespace ServerLibrary.Repositories.Contracts
{
    public interface ICourseInfo
    {
        Task<ResponseRequest<Course>> GetCourse(string name);
    }
}

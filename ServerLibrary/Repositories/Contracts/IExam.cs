using BaseLibrary.Entities;
using BaseLibrary.Responses;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IExam
    {
        Task<ResponseRequest<Schedule>> CheckCourse(int userId, string course);
        Task<ResponseRequest<Exam>> GetInfoExam(int userId, string course);
        Task<ResponseRequest<ExamGrade>> GetExamGrade(int examId);
    }
}

using BaseLibrary.DTOs;
using BaseLibrary.Responses;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IEnrollmentStudent
    {
        Task<ResponseList<EnrollmentStudentProfile>> GetEnrollmentDetails(int userId);
    }
}

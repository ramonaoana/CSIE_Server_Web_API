using BaseLibrary.Responses;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IStudentCertificate
    {
        Task<DocumentResponse> GenerateDocument(int user, string reason);
        Task<GeneralResponse> CheckStatus(int studentId);
        Task<GeneralResponse> CheckStatusUser(int userId);
        Task<StudentInfoResponse> GetStudentInfo(int userId);
        Task<EnrollmentInfoResponse> GetEnrollementDetails(int groupId);
        Task<GeneralResponse> GetDocumentId();
        Task<GeneralResponse> SaveDocument(int id, byte[] content);
    }
}

using BaseLibrary.Responses;
using ServerLibrary.Helpers;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IUserProfile
    {
        Task<UserResponse> GetProfile(int userId);
        Task<ResponseRequest<UserNameAndEmail>> GetName(int userId);
        Task<ResponseRequest<string>> CheckUserRole(int userId);

        Task<ResponseList<CourseProfessor>> GetCoursesForProfessor(int userId);

    }
}

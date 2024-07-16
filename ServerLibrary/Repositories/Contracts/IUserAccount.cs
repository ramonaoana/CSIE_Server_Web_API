using BaseLibrary.DTOs;
using BaseLibrary.Responses;
namespace ServerLibrary.Repositories.Contracts
{
    public interface IUserAccount
    {
        Task<GeneralResponse> CreateAsync(Register user);
        Task<GeneralResponse> InsertUsers(List<UserAccount> users);
        Task<GeneralResponse> InsertProfessors(List<ProfessorAccount> professors);
        Task<Person> GetProfile(int user);
        Task<LoginResponse> SignInAsync(Login user);
        Task<GeneralResponse> LogOut(int user);
        Task<LoginResponse> RefreshTokenAsync(RefreshToken token);
    }
}

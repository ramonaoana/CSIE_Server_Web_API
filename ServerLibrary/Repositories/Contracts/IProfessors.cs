using BaseLibrary.Entities;
using BaseLibrary.Responses;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IProfessors
    {
        Task<ResponseList<Professor>> GetProfessors();
        Task<ResponseRequest<string>> GetEmailAddress(string name);
    }
}

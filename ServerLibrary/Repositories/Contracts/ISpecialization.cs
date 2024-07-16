using BaseLibrary.Entities;
using BaseLibrary.Responses;

namespace ServerLibrary.Repositories.Contracts
{
    public interface ISpecialization
    {
        Task<ResponseList<Specialization>> GetSpecializations();

    }
}

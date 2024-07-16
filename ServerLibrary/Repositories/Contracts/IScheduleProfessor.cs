using BaseLibrary.DTOs;
using BaseLibrary.Responses;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IScheduleProfessor
    {
        Task<ResponseList<InfoSchedule>> GetSchedule(string professorName);
        Task<GeneralResponse> CheckProfessor(string professorName);
    }
}

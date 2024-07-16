using BaseLibrary.Responses;
using ServerLibrary.Helpers;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IGroupSeries
    {
        Task<ResponseRequest<InfoSeriesNameGroups>> GetGroupsForSeries(int userId, string series);
        Task<ResponseList<InfoSeriesNameGroups>> GetGroupsAndSeries(int userId);
        Task<ResponseRequest<string>> GetSeriesForGroup(int userId, int groupNr);
        Task<ResponseList<InfoSeriesNameGroups>> GetGroupsBySpecializationAndYear(int userId, string specialization, int year);
        Task<ResponseList<InfoSeriesNameGroups>> GetGroupsBySpecCycleAndYear(int userId, string specialization, string cycle, int year);
        Task<ResponseList<InfoSeriesSpecialization>> GetSeriesForSpecialization(int userId, string specialization);
    }
}

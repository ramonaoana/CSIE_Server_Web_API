using BaseLibrary.Responses;
using ServerLibrary.Helpers;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IJobOpenings
    {
        Task<ResponseList<AnnouncementInfo>> GetJobOpenings();
    }
}

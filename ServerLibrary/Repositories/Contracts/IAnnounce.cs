using BaseLibrary.Responses;
using ServerLibrary.Helpers;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IAnnounce
    {
        Task<ResponseRequest<AnnouncementInfo>> GetAnnounce(string word);
    }
}

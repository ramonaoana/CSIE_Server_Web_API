using BaseLibrary.Responses;
using ServerLibrary.Helpers;

namespace ServerLibrary.Repositories.Contracts
{
    public interface ILeadership
    {
        Task<ResponseList<MemberLeadershipInfo>> GetUniveristyLeadership();
    }
}

using BaseLibrary.Responses;
using ServerLibrary.Helpers;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IEmail
    {
        Task<GeneralResponse> SendEmail(EmailDataMethod data);
    }
}

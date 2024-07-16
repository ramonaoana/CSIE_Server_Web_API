using BaseLibrary.Responses;

namespace ClientLibrary.Services.Contracts
{
    public interface IChatService
    {

        Task<GeneralResponse> ClassifyMessage(string message, string baseURL);
    }
}

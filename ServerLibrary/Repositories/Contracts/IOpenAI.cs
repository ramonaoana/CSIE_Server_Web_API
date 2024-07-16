using BaseLibrary.DTOs;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IOpenAI
    {
        Task<string> CompleteSentence(OpenAIRequest requestOpenAI);
    }
}

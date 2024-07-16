using BaseLibrary.DTOs;
using Newtonsoft.Json;
using ServerLibrary.Repositories.Contracts;
using System.Text;

namespace ServerLibrary.Repositories.Implementations
{
    public class OpenAIRepository : IOpenAI
    {
        private static readonly HttpClient client = new HttpClient();
        private static string openAIKey = "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";
        public static string endPoint = "https://api.openai.com/v1/chat/completions";
        public static string modelType = "gpt-3.5-turbo-0125";
        public static int maxToxens = 256;
        public static double temp = 0.1f;
        public async Task<string> CompleteSentence(OpenAIRequest requestOpenAI)
        {
            var requestBody = new
            {
                model = modelType,
                messages = new[]{
                    new {role="system",content="You are a helpful assistant"},
                    new {role="user",content=requestOpenAI.Prompt+requestOpenAI.Content},
                }
            };
            string jsonPayload = JsonConvert.SerializeObject(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, endPoint);
            request.Headers.Add("Authorization", $"Bearer {openAIKey}");
            //request.Headers.Add("OpenAI-Beta", "assistants=v1");
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var httpResponse = await client.SendAsync(request);
            string responseContent = await httpResponse.Content.ReadAsStringAsync();
            return responseContent;
        }
    }
}

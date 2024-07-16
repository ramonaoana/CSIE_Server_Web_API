namespace BaseLibrary.DTOs
{
    public class ChatResponse
    {
        public int ResponseType { get; set; }
        public string Response { get; set; }
        public byte[]? Document { get; set; }
    }
}

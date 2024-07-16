namespace ServerLibrary.Helpers
{
    public class AnnouncementInfo
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PostedDate { get; set; }
        public string Type { get; set; }
        public int? DocumentId { get; set; }
        public byte[]? DocumentContent { get; set; }
    }
}

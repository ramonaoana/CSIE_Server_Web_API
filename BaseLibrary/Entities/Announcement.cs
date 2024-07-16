using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; }
        public int? DocumentId { get; set; }
        public string Title { get; set; }
        public string AnnouncementContent { get; set; }
        public string PostedBy { get; set; }
        public DateTime PostedDate { get; set; }
        public byte[] PictureData { get; set; }
        public string Type { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class Document
    {
        [Key]
        public int DocumentId { get; set; }
        public string DocumentName { get; set; }
        public byte[] Content { get; set; }
        public int Type { get; set; }

    }
}

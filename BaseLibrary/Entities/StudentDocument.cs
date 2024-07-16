using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class StudentDocument
    {
        [Key]
        public int StudentDocumentId { get; set; }
        public int StudentId { get; set; }
        public string Name { get; set; }
        public byte[] Content { get; set; }
        public DateTime Date { get; set; }
    }
}

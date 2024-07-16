using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class Promotion
    {
        [Key]
        public int Id { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
    }
}

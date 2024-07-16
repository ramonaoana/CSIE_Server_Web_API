using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class Group
    {
        [Key]
        public int GroupId { get; set; }
        public int SeriesId { get; set; }
        public int GroupNumber { get; set; }
    }
}

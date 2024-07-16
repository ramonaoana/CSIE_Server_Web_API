using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class Series
    {
        [Key]
        public int SeriesId { get; set; }
        public int AcademicYearId { get; set; }
        public string SeriesName { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class Semester
    {
        [Key]
        public int SemesterId { get; set; }
        public int AcademicYearId { get; set; }
        public int SemesterNumber { get; set; }

    }
}

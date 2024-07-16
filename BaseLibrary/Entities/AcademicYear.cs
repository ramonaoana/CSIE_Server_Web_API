using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class AcademicYear
    {
        [Key]
        public int AcademicYearId { get; set; }
        public int PromotionId { get; set; }
        public int SpecializationId { get; set; }
        public int EducationFormId { get; set; }
        public int Year { get; set; }
        public int AcademicStartYear { get; set; }
        public int AcademicEndYear { get; set; }




    }
}

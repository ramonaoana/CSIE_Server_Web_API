using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class ExamGrade
    {
        [Key]
        public int ExamGradeId { get; set; }
        public int ExamId { get; set; }
        public int StudentId { get; set; }
        public int Grade { get; set; }
        public DateTime UploadDate { get; set; }

    }
}

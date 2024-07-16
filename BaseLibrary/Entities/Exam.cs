using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class Exam
    {
        [Key]
        public int ExamId { get; set; }
        public int ScheduleId { get; set; }
        public DateTime ExamDate { get; set; }
        public int Room { get; set; }
    }
}

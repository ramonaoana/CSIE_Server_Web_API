using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class Schedule
    {
        public int ScheduleId { get; set; }
        public int ProfessorId { get; set; }
        public int CourseId { get; set; }
        public int SemesterId { get; set; }
        public string DayOfWeek { get; set; }
        public DateTime Hour { get; set; }
        public int Room { get; set; }
        [MaxLength(1)]
        public string Type { get; set; }

    }
}

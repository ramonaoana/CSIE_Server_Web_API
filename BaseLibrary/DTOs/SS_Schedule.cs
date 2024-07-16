using BaseLibrary.Entities;

namespace BaseLibrary.DTOs
{
    public class SS_Schedule
    {
        public Entities.Group Group { get; set; }
        public Professor Professor { get; set; }
        public Course Course { get; set; }
        public Schedule Schedule { get; set; }
    }
}

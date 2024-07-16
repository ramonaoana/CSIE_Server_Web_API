namespace ServerLibrary.Helpers
{
    public class InfoScheduleForSeries
    {
        public string DayOfWeek { get; set; }
        public string Hour { get; set; }
        public string CourseType { get; set; }
        public string CourseName { get; set; }
        public string ProfessorName { get; set; }
        public string Room { get; set; }
        public List<int> Groups { get; set; }
    }
}

namespace BaseLibrary.DTOs
{
    public class EnrollmentStudentProfile : EnrollmentDetails
    {
        public int Semester { get; set; }
        public string Serie { get; set; }
        public int Group { get; set; }
        public int Status { get; set; }
        public EnrollmentStudentProfile() { }
    }
}

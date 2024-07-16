using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class Professor
    {
        [Key]
        public int ProfessorId { get; set; }
        public int UserId { get; set; }
        public int AddressId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string PersonalEmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string CNP { get; set; }
        public string? AcademicTitle { get; set; }
    }
}

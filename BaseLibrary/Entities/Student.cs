using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }
        public int UserId { get; set; }
        public int AddressId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string PersonalEmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string CNP { get; set; }
    }
}

using BaseLibrary.Entities;

namespace BaseLibrary.DTOs
{
    [Serializable]
    public class UserProfile
    {
        //[Required]
        //public string Id { get; set; } = string.Empty;
        //[Required, EmailAddress, DataType(DataType.EmailAddress)]
        //public string Email { get; set; } = string.Empty;
        public Address Address { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string PersonalEmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string CNP { get; set; }
        public string? Title { get; set; }
    }
}

namespace BaseLibrary.DTOs
{
    public class Person
    {
        public int PersonId { get; set; }
        public string Type { get; set; }
        public int AddressId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string CNP { get; set; }
        public string? AcademicTitle { get; set; }

        public Person()
        {

        }

        public Person(int personId, string type, int addressId, string firstName, string lastName, DateTime dateOfBirth, string placeOfBirth, string emailAddress, string phoneNumber, string cNP, string? academicTitle)
        {
            PersonId = personId;
            Type = type;
            AddressId = addressId;
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
            PlaceOfBirth = placeOfBirth;
            EmailAddress = emailAddress;
            PhoneNumber = phoneNumber;
            CNP = cNP;
            AcademicTitle = academicTitle;
        }
    }
}

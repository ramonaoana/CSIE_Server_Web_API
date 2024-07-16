using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    [Serializable]
    public class Address
    {
        [Key]
        public int AddressId { get; set; }
        public string County { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
        public string PostalCode { get; set; }
    }
}

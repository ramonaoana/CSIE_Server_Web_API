using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class Specialization
    {
        [Key]
        public int SpecializationId { get; set; }
        public int StudyCycleId { get; set; }
        public string SpecializationName { get; set; }
        public int Duration { get; set; }

    }
}

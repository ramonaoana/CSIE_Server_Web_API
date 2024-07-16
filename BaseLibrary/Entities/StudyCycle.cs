using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class StudyCycle
    {
        [Key]
        public int StudyCycleId { get; set; }
        public string Name { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class UniversityLeader
    {
        [Key]
        public int Id { get; set; }
        public int? ProfessorId { get; set; }
        public string Name { get; set; }
        public string Function { get; set; }
        public string Department { get; set; }
        public byte[] Image { get; set; }
    }
}

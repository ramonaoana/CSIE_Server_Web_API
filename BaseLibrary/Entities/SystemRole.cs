using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class SystemRole
    {
        [Key]
        public int RoleId { get; set; }
        [Required]
        public string? Name { get; set; }
    }
}

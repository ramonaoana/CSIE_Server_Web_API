using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class EducationForm
    {
        [Key]
        public int EducationFormId { get; set; }
        public string EducationFormName { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class Language
    {
        [Key]
        public int LanguageId { get; set; }
        public string TeachingLanguage { get; set; }
    }
}

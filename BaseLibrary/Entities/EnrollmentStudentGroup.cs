using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class EnrollmentStudentGroup
    {
        [Key]
        public int EnrollmentStudentGroupId { get; set; }
        public int StudentId { get; set; }
        public int GroupId { get; set; }
        public int FinanceFormId { get; set; }
        public int LanguageId { get; set; }
        public int Status { get; set; }


    }
}

using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities
{
    public class FinanceForm
    {
        [Key]
        public int FinanceFormId { get; set; }
        public string FormFinance { get; set; }
    }
}

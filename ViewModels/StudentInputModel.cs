using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SinglePageApplication.ViewModels
{
    public class StudentInputModel
    {
        public int StudentId { get; set; }

        [Required, StringLength(50)]
        public string StudentName { get; set; } = default!;
        [Required, DataType(DataType.Currency), DisplayFormat(DataFormatString = "{0:0.00}")]
        public decimal AnnualCost { get; set; }
        [Required]
        public IFormFile Picture { get; set; } = default!;
        public bool Continuing { get; set; }
        
    }
}

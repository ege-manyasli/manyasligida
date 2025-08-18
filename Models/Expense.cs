using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using manyasligida.Services;

namespace manyasligida.Models
{
    public class Expense
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;
        
        [Required]
        public DateTime Date { get; set; }
        
        public DateTime ExpenseDate { get; set; } = DateTimeHelper.NowTurkey;
        
        public DateTime CreatedAt { get; set; } = DateTimeHelper.NowTurkey;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

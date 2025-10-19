using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 1000000, ErrorMessage = "Amount must be between 0.01 and 1,000,000")]
        [DataType(DataType.Currency)]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class MonthlySummary
    {
        public string Month { get; set; }
        public decimal TotalAmount { get; set; }
        public Dictionary<string, decimal> CategoryBreakdown { get; set; }
    }

    public class CategorySummary
    {
        public string Category { get; set; }
        public decimal TotalAmount { get; set; }
        public int Count { get; set; }
    }
}
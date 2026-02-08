using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.DTOs
{
    public class CreateInventoryItemDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int MinimumStock { get; set; }

        public string? Location { get; set; }
    }

    public class UpdateInventoryItemDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue)]
        public int? MinimumStock { get; set; }

        public string? Location { get; set; }
    }

    public class StockTransactionDto
    {
        [Required]
        public int InventoryItemId { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty; // "StockIn" or "StockOut"

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        [StringLength(100)]
        public string PerformedBy { get; set; } = "System";
    }

    public class DashboardStatsDto
    {
        public int TotalItems { get; set; }
        public int LowStockItems { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int OutOfStockItems { get; set; }
        public List<CategorySummary> CategoryBreakdown { get; set; } = new();
        public List<RecentActivity> RecentTransactions { get; set; } = new();
    }

    public class CategorySummary
    {
        public string Category { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class RecentActivity
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.Models
{
    public class StockTransaction
    {
        public int Id { get; set; }

        [Required]
        public int InventoryItemId { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string PerformedBy { get; set; } = "System";

        public int PreviousQuantity { get; set; }

        public int NewQuantity { get; set; }

        // Navigation property
        public virtual InventoryItem? InventoryItem { get; set; }
    }

    public enum TransactionType
    {
        StockIn,
        StockOut,
        Adjustment,
        InitialStock
    }
}

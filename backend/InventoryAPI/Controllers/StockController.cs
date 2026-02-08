using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryAPI.Data;
using InventoryAPI.Models;
using InventoryAPI.DTOs;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        private readonly ILogger<StockController> _logger;

        public StockController(InventoryDbContext context, ILogger<StockController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/stock/transaction
        [HttpPost("transaction")]
        public async Task<ActionResult<StockTransaction>> ProcessStockTransaction(StockTransactionDto dto)
        {
            try
            {
                var item = await _context.InventoryItems.FindAsync(dto.InventoryItemId);
                if (item == null)
                {
                    return NotFound(new { message = $"Item with ID {dto.InventoryItemId} not found" });
                }

                var previousQuantity = item.Quantity;
                var transactionType = Enum.Parse<TransactionType>(dto.Type);
                int newQuantity;

                // Calculate new quantity based on transaction type
                switch (transactionType)
                {
                    case TransactionType.StockIn:
                        newQuantity = previousQuantity + dto.Quantity;
                        break;
                    case TransactionType.StockOut:
                        if (previousQuantity < dto.Quantity)
                        {
                            return BadRequest(new { 
                                message = $"Insufficient stock. Available: {previousQuantity}, Requested: {dto.Quantity}" 
                            });
                        }
                        newQuantity = previousQuantity - dto.Quantity;
                        break;
                    case TransactionType.Adjustment:
                        newQuantity = dto.Quantity; // For adjustments, quantity is the new value
                        break;
                    default:
                        return BadRequest(new { message = "Invalid transaction type" });
                }

                // Create transaction record
                var transaction = new StockTransaction
                {
                    InventoryItemId = dto.InventoryItemId,
                    Type = transactionType,
                    Quantity = dto.Quantity,
                    PreviousQuantity = previousQuantity,
                    NewQuantity = newQuantity,
                    Notes = dto.Notes,
                    PerformedBy = dto.PerformedBy,
                    TransactionDate = DateTime.UtcNow
                };

                // Update item quantity
                item.Quantity = newQuantity;
                item.LastUpdated = DateTime.UtcNow;

                _context.StockTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                // Check for low stock alert
                var alert = CheckLowStockAlert(item);
                
                return Ok(new
                {
                    transaction,
                    item,
                    alert
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stock transaction");
                return StatusCode(500, "An error occurred while processing the transaction");
            }
        }

        // GET: api/stock/transactions/{itemId}
        [HttpGet("transactions/{itemId}")]
        public async Task<ActionResult<IEnumerable<StockTransaction>>> GetItemTransactions(
            int itemId, 
            [FromQuery] int limit = 50)
        {
            try
            {
                var transactions = await _context.StockTransactions
                    .Where(t => t.InventoryItemId == itemId)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(limit)
                    .ToListAsync();

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transactions for item {ItemId}", itemId);
                return StatusCode(500, "An error occurred while fetching transactions");
            }
        }

        // GET: api/stock/transactions
        [HttpGet("transactions")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllTransactions([FromQuery] int limit = 100)
        {
            try
            {
                var transactions = await _context.StockTransactions
                    .Include(t => t.InventoryItem)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(limit)
                    .Select(t => new
                    {
                        t.Id,
                        t.InventoryItemId,
                        ItemName = t.InventoryItem!.Name,
                        ItemSKU = t.InventoryItem.SKU,
                        t.Type,
                        t.Quantity,
                        t.PreviousQuantity,
                        t.NewQuantity,
                        t.Notes,
                        t.PerformedBy,
                        t.TransactionDate
                    })
                    .ToListAsync();

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all transactions");
                return StatusCode(500, "An error occurred while fetching transactions");
            }
        }

        // Helper method to check low stock and trigger alert
        private object? CheckLowStockAlert(InventoryItem item)
        {
            if (item.IsLowStock)
            {
                _logger.LogWarning(
                    "Low stock alert: {ItemName} (SKU: {SKU}) - Current: {Current}, Minimum: {Minimum}",
                    item.Name, item.SKU, item.Quantity, item.MinimumStock);

                return new
                {
                    isLowStock = true,
                    message = $"Low stock alert: {item.Name} has only {item.Quantity} units remaining (minimum: {item.MinimumStock})",
                    itemName = item.Name,
                    sku = item.SKU,
                    currentQuantity = item.Quantity,
                    minimumStock = item.MinimumStock
                };
            }

            return null;
        }
    }
}

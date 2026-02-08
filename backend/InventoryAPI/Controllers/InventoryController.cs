using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryAPI.Data;
using InventoryAPI.Models;
using InventoryAPI.DTOs;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(InventoryDbContext context, ILogger<InventoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/inventory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetAllItems(
            [FromQuery] string? category = null,
            [FromQuery] bool? lowStockOnly = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.InventoryItems.AsQueryable();

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(i => i.Category == category);
                }

                if (lowStockOnly == true)
                {
                    query = query.Where(i => i.Quantity <= i.MinimumStock);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(i => i.Name.Contains(search) || 
                                           i.SKU.Contains(search) || 
                                           (i.Description != null && i.Description.Contains(search)));
                }

                var items = await query.OrderBy(i => i.Name).ToListAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching inventory items");
                return StatusCode(500, "An error occurred while fetching items");
            }
        }

        // GET: api/inventory/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryItem>> GetItem(int id)
        {
            try
            {
                var item = await _context.InventoryItems
                    .Include(i => i.StockTransactions.OrderByDescending(t => t.TransactionDate).Take(10))
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (item == null)
                {
                    return NotFound(new { message = $"Item with ID {id} not found" });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching item {Id}", id);
                return StatusCode(500, "An error occurred while fetching the item");
            }
        }

        // POST: api/inventory
        [HttpPost]
        public async Task<ActionResult<InventoryItem>> CreateItem(CreateInventoryItemDto dto)
        {
            try
            {
                // Check if SKU already exists
                if (await _context.InventoryItems.AnyAsync(i => i.SKU == dto.SKU))
                {
                    return BadRequest(new { message = $"SKU '{dto.SKU}' already exists" });
                }

                var item = new InventoryItem
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    SKU = dto.SKU,
                    Category = dto.Category,
                    Price = dto.Price,
                    Quantity = dto.Quantity,
                    MinimumStock = dto.MinimumStock,
                    Location = dto.Location,
                    CreatedAt = DateTime.UtcNow
                };

                _context.InventoryItems.Add(item);
                await _context.SaveChangesAsync();

                // Create initial stock transaction
                var transaction = new StockTransaction
                {
                    InventoryItemId = item.Id,
                    Type = TransactionType.InitialStock,
                    Quantity = item.Quantity,
                    PreviousQuantity = 0,
                    NewQuantity = item.Quantity,
                    Notes = "Initial stock entry",
                    PerformedBy = "System",
                    TransactionDate = DateTime.UtcNow
                };

                _context.StockTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inventory item");
                return StatusCode(500, "An error occurred while creating the item");
            }
        }

        // PUT: api/inventory/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, UpdateInventoryItemDto dto)
        {
            try
            {
                var item = await _context.InventoryItems.FindAsync(id);
                if (item == null)
                {
                    return NotFound(new { message = $"Item with ID {id} not found" });
                }

                // Update only provided fields
                if (dto.Name != null) item.Name = dto.Name;
                if (dto.Description != null) item.Description = dto.Description;
                if (dto.Category != null) item.Category = dto.Category;
                if (dto.Price.HasValue) item.Price = dto.Price.Value;
                if (dto.MinimumStock.HasValue) item.MinimumStock = dto.MinimumStock.Value;
                if (dto.Location != null) item.Location = dto.Location;

                item.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item {Id}", id);
                return StatusCode(500, "An error occurred while updating the item");
            }
        }

        // DELETE: api/inventory/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                var item = await _context.InventoryItems.FindAsync(id);
                if (item == null)
                {
                    return NotFound(new { message = $"Item with ID {id} not found" });
                }

                _context.InventoryItems.Remove(item);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item {Id}", id);
                return StatusCode(500, "An error occurred while deleting the item");
            }
        }

        // GET: api/inventory/categories
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            try
            {
                var categories = await _context.InventoryItems
                    .Select(i => i.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories");
                return StatusCode(500, "An error occurred while fetching categories");
            }
        }

        // GET: api/inventory/low-stock
        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetLowStockItems()
        {
            try
            {
                var items = await _context.InventoryItems
                    .Where(i => i.Quantity <= i.MinimumStock)
                    .OrderBy(i => i.Quantity)
                    .ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching low stock items");
                return StatusCode(500, "An error occurred while fetching low stock items");
            }
        }
    }
}

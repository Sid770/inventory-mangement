using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryAPI.Data;
using InventoryAPI.DTOs;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(InventoryDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/dashboard/stats
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            try
            {
                var items = await _context.InventoryItems.ToListAsync();
                
                var stats = new DashboardStatsDto
                {
                    TotalItems = items.Count,
                    LowStockItems = items.Count(i => i.IsLowStock && i.Quantity > 0),
                    OutOfStockItems = items.Count(i => i.Quantity == 0),
                    TotalInventoryValue = items.Sum(i => i.Price * i.Quantity),
                    
                    CategoryBreakdown = items
                        .GroupBy(i => i.Category)
                        .Select(g => new CategorySummary
                        {
                            Category = g.Key,
                            ItemCount = g.Count(),
                            TotalValue = g.Sum(i => i.Price * i.Quantity)
                        })
                        .OrderByDescending(c => c.TotalValue)
                        .ToList(),
                    
                    RecentTransactions = await _context.StockTransactions
                        .Include(t => t.InventoryItem)
                        .OrderByDescending(t => t.TransactionDate)
                        .Take(10)
                        .Select(t => new RecentActivity
                        {
                            Id = t.Id,
                            ItemName = t.InventoryItem!.Name,
                            Type = t.Type.ToString(),
                            Quantity = t.Quantity,
                            Date = t.TransactionDate,
                            PerformedBy = t.PerformedBy
                        })
                        .ToListAsync()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard stats");
                return StatusCode(500, "An error occurred while fetching dashboard statistics");
            }
        }

        // GET: api/dashboard/alerts
        [HttpGet("alerts")]
        public async Task<ActionResult> GetAlerts()
        {
            try
            {
                var lowStockItems = await _context.InventoryItems
                    .Where(i => i.Quantity <= i.MinimumStock)
                    .OrderBy(i => i.Quantity)
                    .Select(i => new
                    {
                        i.Id,
                        i.Name,
                        i.SKU,
                        i.Quantity,
                        i.MinimumStock,
                        i.Category,
                        alertLevel = i.Quantity == 0 ? "critical" : 
                                    i.Quantity < i.MinimumStock / 2 ? "high" : "medium"
                    })
                    .ToListAsync();

                return Ok(new
                {
                    totalAlerts = lowStockItems.Count,
                    criticalAlerts = lowStockItems.Count(a => a.Quantity == 0),
                    alerts = lowStockItems
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching alerts");
                return StatusCode(500, "An error occurred while fetching alerts");
            }
        }
    }
}

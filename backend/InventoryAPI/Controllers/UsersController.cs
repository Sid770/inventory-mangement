using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using InventoryAPI.Models;

namespace InventoryAPI.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly TableClient _table;

    public UsersController(TableServiceClient serviceClient)
    {
        _table = serviceClient.GetTableClient("Users");
        _table.CreateIfNotExists();
    }

    [HttpPost]
    public async Task<IActionResult> AddUser(UserEntity user)
    {
        await _table.AddEntityAsync(user);
        return Ok(user);
    }

    [HttpGet]
    public Pageable<UserEntity> GetAllUsers()
    {
        return _table.Query<UserEntity>();
    }
}

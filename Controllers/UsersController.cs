using Microsoft.AspNetCore.Mvc;
using ProteinOnWheelsAPI.Data;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ProteinOnWheelsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    //  GET ALL USERS (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult GetUsers()
    {
        var users = _context.Users
            .OrderBy(u => u.Name)
            .Select(u => new
            {
                u.Name,
                u.Email,

                //get phone from Orders table
                Phone = _context.Orders
                    .Where(o => o.UserId == u.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => o.PhoneNumber)
                    .FirstOrDefault(),

                //get address from Orders table
                Address = _context.Orders
                    .Where(o => o.UserId == u.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => o.Address)
                    .FirstOrDefault()
            })
            .ToList();

        return Ok(users);
    }
}
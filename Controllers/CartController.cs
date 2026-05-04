using Microsoft.AspNetCore.Mvc;
using ProteinOnWheelsAPI.Data;
using ProteinOnWheelsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


namespace ProteinOnWheelsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpPost("add")]
    public IActionResult AddToCart(Cart item)
    {
        var userId = int.Parse(User.FindFirst("id")?.Value);
        item.UserId = userId;


        var existing = _context.CartItems
            .FirstOrDefault(c => c.UserId == userId  && c.ProductId == item.ProductId);

        if (existing != null)
        {
            existing.Quantity += item.Quantity;
        }
        else
        {
            _context.CartItems.Add(item);
        }

        _context.SaveChanges();

        return Ok();
    }

    
    [HttpGet("my-cart")]
    public IActionResult GetCart()
    {
        var userId = int.Parse(User.FindFirst("id")?.Value);
        var cart = _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToList();

        return Ok(cart);
    }

    [HttpDelete("{id}")]
    public IActionResult RemoveItem(int id)
    {
        var item = _context.CartItems.Find(id);

        if (item == null)
            return NotFound();

        _context.CartItems.Remove(item);
        _context.SaveChanges();

        return Ok("Item removed");
    } 
}
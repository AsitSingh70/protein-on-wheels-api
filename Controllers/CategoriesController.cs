using Microsoft.AspNetCore.Mvc;
using ProteinOnWheelsAPI.Data;
using ProteinOnWheelsAPI.Models;

namespace ProteinOnWheelsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public IActionResult AddCategory(Category category)
    {
        _context.Categories.Add(category);
        _context.SaveChanges();

        return Ok(category);
    }

    [HttpGet]
    public IActionResult GetCategories()
    {
        return Ok(_context.Categories.ToList());
    }
}
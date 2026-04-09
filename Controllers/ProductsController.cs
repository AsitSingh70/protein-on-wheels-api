using Microsoft.AspNetCore.Mvc;
using ProteinOnWheelsAPI.Data;
using ProteinOnWheelsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ProteinOnWheelsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetProducts()
    {

        // var products = _context.Products.ToList();
        // return Ok(products);

        //updated here to add category name 
        var products = _context.Products.Include(p => p.Category)
        .ToList();
        return Ok(products);


    }

    [HttpGet("{id}")]
    public IActionResult GetProduct(int id)
    {

        // var product = _context.Products.Find(id);
        // return Ok(product);

        //updated here to add category name
        var product = _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);
        return Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult AddProduct(Product product)
    {

        var category = _context.Categories.Find(product.CategoryId);

        if (category == null)
            return BadRequest("Category not found");

        product.Category = category;

        _context.Products.Add(product);
        _context.SaveChanges();

        return Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public IActionResult UpdateProduct(int id, Product updated)
    {

        //updated here to add category name
        // var product = _context.Products.Find(id);
        var product = _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

        if (product == null)
            return NotFound();

        product.Name = updated.Name;
        product.Price = updated.Price;
        product.Description = updated.Description;
        product.ImageUrl = updated.ImageUrl;
        product.CategoryId = updated.CategoryId;

        _context.SaveChanges();

        return Ok(product);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(int id)
    {
        var product = _context.Products.Find(id);

        if (product == null)
            return NotFound();

        _context.Products.Remove(product);
        _context.SaveChanges();

        return Ok("Deleted");
    }


    //A website will hit this endpoint to keep backend alive becuse backend sleeps in every 15 min if request not comes..
    [HttpGet("alive")]
    public IActionResult Health()
    {
        return Ok("API Running");
    }

}
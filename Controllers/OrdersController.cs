using Microsoft.AspNetCore.Mvc;
using ProteinOnWheelsAPI.Data;
using ProteinOnWheelsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


namespace ProteinOnWheelsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpPost("place")]
    public IActionResult PlaceOrder([FromBody] CreateOrderDTO dto)
    {
        // var userId = int.Parse(User.FindFirst("id")?.Value);

        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim == null)
            return Unauthorized("Invalid token");

        var userId = int.Parse(userIdClaim);

        if (string.IsNullOrEmpty(dto.PhoneNumber) || string.IsNullOrEmpty(dto.Address))
            return BadRequest("Phone and Address required");

        var cartItems = _context.CartItems
            .Where(c => c.UserId == userId)
            .ToList();

        if (!cartItems.Any())
            return BadRequest("Cart empty");

        var order = new Order
        {
            UserId = userId,
            TotalAmount = 0,
            Status = "Pending",
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            Items = new List<OrderItem>()
        };

        foreach (var item in cartItems)
        {
            var product = _context.Products.Find(item.ProductId);

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                Price = product.Price
            };

            order.Items.Add(orderItem);

            order.TotalAmount += product.Price * item.Quantity;
        }

        _context.Orders.Add(order);

        _context.CartItems.RemoveRange(cartItems);

        _context.SaveChanges();

        return Ok(order);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult GetOrders()
    {
        var orders = _context.Orders
        .Include(o => o.Items)
            .ThenInclude(i => i.Product)
        .Include(o => o.User)
        .OrderByDescending(o => o.OrderDate)
        .ToList();

        var result = orders.Select(o => new
        {
            o.Id,
            o.TotalAmount,
            o.Status,
            o.OrderDate,
            o.PhoneNumber,
            o.Address,

            UserName = o.User.Name,
            UserEmail = o.User.Email,

            Items = o.Items.Select(i => new
            {
                ProductName = i.Product.Name,
                i.Quantity
            })
        });


        return Ok(result);
    }

    //get orders for a user(order history)
    [Authorize]
    [HttpGet("my-orders")]
    public IActionResult GetMyOrders()
    {
        //  get userId from JWT
        var userId = int.Parse(User.FindFirst("id")?.Value);

        //get orders of this user
        var orders = _context.Orders
            .Where(o => o.UserId == userId) //filter by logged-in user
            .OrderByDescending(o => o.OrderDate) 
            .Select(o => new
            {
                o.Id,
                o.TotalAmount,
                o.OrderDate,
                o.Status,

                //include items
                Items = o.Items.Select(i => new
                {
                    i.ProductId,
                    i.Quantity,
                    i.Price,
                    ProductName = i.Product.Name //get product name
                })
            })
            .ToList();

        return Ok(orders);
    }

    [Authorize(Roles = "Admin")] //only admin
    [HttpPut("update-status/{orderId}")]
    public IActionResult UpdateOrderStatus(int orderId, [FromBody] string status)
    {
        var order = _context.Orders.Find(orderId);

        if (order == null)
            return NotFound("Order not found");

        //UPDATE status
        order.Status = status; //changed by admin

        _context.SaveChanges();

        return Ok("Order status updated");
    }

    [Authorize(Roles = "Admin")] //only admin
    [HttpGet("report")]
    public IActionResult GetMonthlyReport(int year, int month)
    {
        //filter orders by year & month
        var orders = _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Where(o => o.OrderDate.Year == year && o.OrderDate.Month == month)
            .OrderByDescending(o => o.OrderDate) // latest first
            .ToList();

        if (!orders.Any())
            return Ok(new
            {
                totalOrders = 0,
                approved = 0,
                pending = 0,
                cancelled = 0,
                totalRevenue = 0,
                orders = new List<object>()
            }); ;

        //calculate counts
        var totalOrders = orders.Count; //total orders

        var approved = orders.Count(o => o.Status == "Approved");
        // var shipped = orders.Count(o => o.Status == "Shipped");   
        // var delivered = orders.Count(o => o.Status == "Delivered");
        var cancelled = orders.Count(o => o.Status == "Cancelled");
        var pending = orders.Count(o => o.Status == "Pending");

        //total revenue (only delivered orders usually)
        var totalRevenue = orders
            .Where(o => o.Status == "Approved") //only approved
            .Sum(o => o.TotalAmount);

        //final response
        var result = new
        {
            totalOrders,
            approved,
            // shipped,
            // delivered,
            pending,
            cancelled,
            totalRevenue,

            //full orders list 🆕
            orders = orders.Select(o => new
            {
                o.Id,
                o.UserId,
                o.TotalAmount,
                o.Status,
                o.OrderDate,

                Items = o.Items.Select(i => new
                {
                    i.ProductId,
                    i.Quantity,
                    i.Price,
                    ProductName = i.Product.Name
                })
            })
        };

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("top-customers")]
    public IActionResult GetTopCustomers(int year, int month)
    {
        var customers = _context.Orders
            .Include(o => o.User)
            .Where(o => o.OrderDate.Year == year && o.OrderDate.Month == month)
            .GroupBy(o => o.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                Name = g.First().User.Name,
                Email = g.First().User.Email,
                Phone = g.Where(x => !string.IsNullOrEmpty(x.PhoneNumber))
                        .Select(x => x.PhoneNumber)
                        .FirstOrDefault(),

                Address = g.Where(x => !string.IsNullOrEmpty(x.Address))
                        .Select(x => x.Address)
                        .FirstOrDefault(),

                TotalOrders = g.Count(),
                TotalSpent = g.Sum(x => x.TotalAmount)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(30) //top 30
            .ToList();

        return Ok(customers);
    }
}
namespace ProteinOnWheelsAPI.Models;

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;

    public List<OrderItem> Items { get; set; }

    public string Status { get; set; } = "Pending";

    public string PhoneNumber { get; set; }
    public string Address { get; set; }
}
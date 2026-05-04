namespace ProteinOnWheelsAPI.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public string Role { get; set; } = "User";

    public string? OtpCode { get; set; }

    public DateTime? OtpExpireTime { get; set; }

    public bool IsEmailVerified { get; set; } = false;

    public bool IsResetOtpVerified { get; set; } = false;


    public int FailedLoginAttempts { get; set; } = 0;  
    public DateTime? LockoutEndTime { get; set; } 
}
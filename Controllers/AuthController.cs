using Microsoft.AspNetCore.Mvc;
using ProteinOnWheelsAPI.Data;
using ProteinOnWheelsAPI.DTOs;
using ProteinOnWheelsAPI.Models;
using ProteinOnWheelsAPI.Services;
using BCrypt.Net;

namespace ProteinOnWheelsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwt;
    private readonly EmailService _email;

    public AuthController(AppDbContext context, JwtService jwt, EmailService email)
    {
        _context = context;
        _jwt = jwt;
        _email = email;
    }

    [HttpPost("register")]
    public IActionResult Register(RegisterDTO dto)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);

        if (user == null)
            return BadRequest("Please verify OTP first");

        if (!user.IsEmailVerified)
            return BadRequest("Email not verified");

        user.Name = dto.Name;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        user.Role = "User";

        user.OtpCode = null;
        user.OtpExpireTime = null;

        _context.SaveChanges();

        return Ok("User created");
    }

    [HttpPost("login")]
    public IActionResult Login(LoginDTO dto)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);

        if (user == null)
            return BadRequest("User not found");

        if (!user.IsEmailVerified)
            return BadRequest("Please verify your email first");

        if (string.IsNullOrEmpty(user.PasswordHash))
            return BadRequest("Please complete registration first");


        //  CHECK IF ACCOUNT IS LOCKED 
        if (user.LockoutEndTime != null && user.LockoutEndTime > DateTime.UtcNow) 
        {
            var remainingTime = (user.LockoutEndTime.Value - DateTime.UtcNow).Minutes; 
            return BadRequest($"Account locked. Try again after {remainingTime} minutes"); 
        }

        bool valid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

        if (!valid)
        {
            user.FailedLoginAttempts++; 

            // LOCK AFTER 5 ATTEMPTS 
            if (user.FailedLoginAttempts >= 5) 
            {
                user.LockoutEndTime = DateTime.UtcNow.AddMinutes(15); 
                user.FailedLoginAttempts = 0; //RESET AFTER LOCK 

                _context.SaveChanges();

                return BadRequest("Too many attempts. Account locked for 15 minutes"); 
            }

            _context.SaveChanges();

            return BadRequest($"Wrong password. Attempts left: {5 - user.FailedLoginAttempts}");
        }


        // SUCCESS LOGIN → RESET EVERYTHING 
        user.FailedLoginAttempts = 0; 
        user.LockoutEndTime = null;  
        _context.SaveChanges();

        var token = _jwt.GenerateToken(user);

        return Ok(new { token });
    }

    [HttpPost("send-otp")]
    public IActionResult SendOtp([FromQuery] string email)
    {
        try
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user != null && user.OtpExpireTime > DateTime.UtcNow)
            {
                return Ok("OTP already sent, please check email");
            }

            var otp = new Random().Next(100000, 999999).ToString();

            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    Name = "Temp",
                    PasswordHash = "Temp@123",
                    IsEmailVerified = false
                };

                _context.Users.Add(user);
            }

            user.OtpCode = otp;
            user.OtpExpireTime = DateTime.UtcNow.AddMinutes(5);

            _context.SaveChanges();

            // 🔥 THIS IS FAILING
            _email.SendOtp(email, otp);

            return Ok("OTP sent to email");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
        }
    }
    // [HttpPost("send-otp")]
    // public IActionResult SendOtp(string email)
    // {

    //     var user = _context.Users.FirstOrDefault(u => u.Email == email);

    //     if (user != null && user.OtpExpireTime > DateTime.Now)
    //     {
    //         return Ok("OTP already sent, please check email");
    //     }


    //     var otp = new Random().Next(100000, 999999).ToString();


    //     //updated here to add email verification for new users
    //     // if (user == null)
    //     // {
    //     //     user = new User { Email = email };
    //     //     _context.Users.Add(user);
    //     // }

    //     if (user == null)
    //     {
    //         user = new User
    //         {
    //             Email = email,
    //             Name = "Temp",
    //             PasswordHash = "Temp123",
    //             IsEmailVerified = false
    //         };

    //         _context.Users.Add(user);
    //     }

    //     user.OtpCode = otp;
    //     user.OtpExpireTime = DateTime.Now.AddMinutes(5);

    //     _context.SaveChanges();

    //     _email.SendOtp(email, otp);

    //     return Ok("OTP sent to email");
    // }

    [HttpPost("verify-otp")]
    public IActionResult VerifyOtp(string email, string otp)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        if (user == null)
            return BadRequest("User not found");

        if (user.OtpCode != otp)
            return BadRequest("Invalid OTP");

        if (user.OtpExpireTime < DateTime.UtcNow)
            return BadRequest("OTP expired");

        user.IsEmailVerified = true;

        user.OtpCode = null;
        user.OtpExpireTime = null;
        _context.SaveChanges();

        return Ok("OTP verified");
    }


    // ================= FORGOT PASSWORD FLOW =================

    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword(string email)
    {
        //check if user exists
        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        if (user == null)
            return BadRequest("User not found");

        //generate OTP
        var otp = new Random().Next(100000, 999999).ToString();



        //UPDATED: store OTP for password reset
        user.OtpCode = otp;
        user.OtpExpireTime = DateTime.UtcNow.AddMinutes(5);

        user.IsResetOtpVerified = false;

        _context.SaveChanges();

        //send email
        _email.SendOtp(email, otp);

        return Ok("Reset OTP sent to email");
    }

    [HttpPost("verify-reset-otp")]
    public IActionResult VerifyResetOtp(string email, string otp)
    {
        //find user
        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        if (user == null)
            return BadRequest("User not found");

        //check OTP
        if (user.OtpCode != otp)
            return BadRequest("Invalid OTP");

        //check expiry
        if (user.OtpExpireTime < DateTime.UtcNow)
            return BadRequest("OTP expired");


        user.IsResetOtpVerified = true;

        _context.SaveChanges();

        //OTP verified
        return Ok("OTP verified for password reset");
    }

    [HttpPost("reset-password")]
    public IActionResult ResetPassword(string email, string newPassword)
    {
        //find user
        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        if (user == null)
            return BadRequest("User not found");

        if (!user.IsResetOtpVerified)
            return BadRequest("OTP not verified for this email!");

        // UPDATED: hash new password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        //clear OTP after reset
        user.OtpCode = null;
        user.OtpExpireTime = null;

        user.IsResetOtpVerified = false;

        _context.SaveChanges();

        return Ok("Password reset successful");
    }
}
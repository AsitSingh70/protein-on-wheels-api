using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace ProteinOnWheelsAPI.Services;

public class EmailService
{

    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }
    public void SendOtp(string email, string otp)
    {
        // Brevo SMTP server
        var smtp = new SmtpClient("smtp-relay.brevo.com")
        {
            Port = 587, // Brevo SMTP port

            Credentials = new NetworkCredential(
                _config["EmailSettings:Email"], // 🔐 from env
                _config["EmailSettings:Password"] // 🔴 CHANGE THIS → your Brevo SMTP key (from Brevo dashboard)
            ),

            EnableSsl = true // secure connection
        };

        // Email message
        var mail = new MailMessage(
             _config["EmailSettings:Email"], // 🔴 CHANGE THIS → same email as above (sender email)
            email,                       // user email (OTP will be sent here)
            "Your OTP Code for login to Protine On Wheels: ",// email subject
            $"{otp}"        // email body (OTP message)
        );

        // Send email
        smtp.Send(mail);
    }
}
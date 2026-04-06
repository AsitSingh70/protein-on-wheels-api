// using System.Net;
// using System.Net.Mail;
// using Microsoft.Extensions.Configuration;

// namespace ProteinOnWheelsAPI.Services;

// public class EmailService
// {

//     private readonly IConfiguration _config;

//     public EmailService(IConfiguration config)
//     {
//         _config = config;
//     }
//     public void SendOtp(string email, string otp)
//     {
//         // Brevo SMTP server
//         var smtp = new SmtpClient("smtp-relay.brevo.com")
//         {
//             Port = 587, // Brevo SMTP port

//             Credentials = new NetworkCredential(
//                 _config["EmailSettings:Email"], // 🔐 from env
//                 _config["EmailSettings:Password"] // 🔴 CHANGE THIS → your Brevo SMTP key (from Brevo dashboard)
//             ),

//             EnableSsl = true // secure connection
//         };

//         // Email message
//         var mail = new MailMessage(
//              "singhasitkumar9@gmail.com", // 🔴 CHANGE THIS → same email as above (sender email)
//             email,                       // user email (OTP will be sent here)
//             "Your OTP Code for login to Protine On Wheels: ",// email subject
//             $"{otp}"        // email body (OTP message)
//         );

//         // Send email
//         try
//         {
//             smtp.Send(mail);
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine("EMAIL ERROR: " + ex.Message);
//             throw;
//         }
//     }
// }

using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ProteinOnWheelsAPI.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendOtp(string email, string otp)
    {
        var client = new HttpClient();

        var apiKey = _config["EmailSettings:ApiKey"];

        var data = new
        {
            sender = new { email = "singhasitkumar9@gmail.com" },
            to = new[] { new { email = email } },
            subject = "Your OTP Code for Protein On Wheels",
            htmlContent = $"<h2>Your OTP is: {otp}</h2>"
        };

        var json = JsonSerializer.Serialize(data);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
        request.Headers.Add("api-key", apiKey);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(error);
        }
    }
}
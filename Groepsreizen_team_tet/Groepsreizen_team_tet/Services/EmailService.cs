using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

public class EmailService
{
    private readonly string _apiKey;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public EmailService(IConfiguration configuration)
    {
        // Haal configuratie op uit appsettings.json
        var emailSettings = configuration.GetSection("EmailSettings");
        _apiKey = emailSettings["SendGridApiKey"];
        _senderEmail = emailSettings["SenderEmail"];
        _senderName = emailSettings["SenderName"];
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress(_senderEmail, _senderName);
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, message, message);

        var response = await client.SendEmailAsync(msg);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Body.ReadAsStringAsync();
            throw new System.Exception($"Fout bij verzenden van e-mail: {response.StatusCode}, {error}");
        }
    }
}

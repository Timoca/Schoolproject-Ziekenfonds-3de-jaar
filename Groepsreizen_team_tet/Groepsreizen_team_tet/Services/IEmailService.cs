namespace Groepsreizen_team_tet.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);

        //to: email van de ontvanger
        //subject: onderwerp van de email
        //body: inhoud van de email

    }
}

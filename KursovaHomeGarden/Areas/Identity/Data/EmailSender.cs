using Microsoft.AspNetCore.Identity.UI.Services;

namespace KursovaHomeGarden.Areas.Identity.Data
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Task.CompletedTask;  
        }
    }
}

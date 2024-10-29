using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace Task_Management_System.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Implement a real email sending mechanism here, e.g., using SMTP or a third-party email API.

            // For now, just return a completed task.
            return Task.CompletedTask;
        }
    }
}

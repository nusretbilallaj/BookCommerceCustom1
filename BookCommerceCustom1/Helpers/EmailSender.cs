using Microsoft.AspNetCore.Identity.UI.Services;

namespace BookCommerceCustom1.Helpers
{
	public class EmailSender:IEmailSender
	{
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Task.CompletedTask;
        }
    }
}

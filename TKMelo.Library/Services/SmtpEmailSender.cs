using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TKMelo.Library.Interfaces;

namespace TKMelo.Library.Services
{
    public class SmtpOptions
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; } = 587;
        public string User { get; set; } = default!;
        public string Password { get; set; } = default!;
        public bool UseStartTls { get; set; } = true;
        public string FromEmail { get; set; } = default!;
        public string FromName { get; set; } = "TKMelo";
        public string FrontendBaseUrl { get; set; } = default!;
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _opt;
        public SmtpEmailSender(IOptions<SmtpOptions> opt) => _opt = opt.Value;

        public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_opt.FromName, _opt.FromEmail));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            msg.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_opt.Host, _opt.Port, SecureSocketOptions.StartTlsWhenAvailable, ct);
            if (!string.IsNullOrWhiteSpace(_opt.User))
                await client.AuthenticateAsync(_opt.User, _opt.Password, ct);

            await client.SendAsync(msg, ct);
            await client.DisconnectAsync(true, ct);
        }
    }
}

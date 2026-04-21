using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using YourApp.Application.Common.Interfaces;
using YourApp.Infrastructure.Configuration;
using YourApp.Domain.Entities;

namespace YourApp.Infrastructure.Services;

public class MailService : IMailService
{
    private readonly MailSettings _settings;

    public MailService(IOptions<MailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendVerificationAsync(User user, string token)
    {
        var verifyUrl = $"{_settings.ClientUrl}/auth/verify-email?token={token}&email={user.Email}";
        var timestamp = DateTimeOffset.UtcNow.ToString("O");
        
        // Đọc tệp Template HTML từ thư mục Domain
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "VerifyMail.html");
        
        // Nếu không tìm thấy trong BaseDirectory (khi debug/run), thử tìm theo đường dẫn tương đối từ project
        if (!File.Exists(templatePath))
        {
            templatePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "YourApp.Domain", "Templates", "VerifyMail.html");
        }

        var htmlBody = await File.ReadAllTextAsync(templatePath);

        // Thay thế các biến trong Template
        htmlBody = htmlBody
            .Replace("{{USERNAME}}", user.Username)
            .Replace("{{VERIFY_URL}}", verifyUrl)
            .Replace("{{ username }}", user.Username)
            .Replace("{{ verify_url }}", verifyUrl)
            .Replace("{{ timestamp }}", timestamp);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));
        message.To.Add(new MailboxAddress(user.Username, user.Email));
        message.Subject = "Xác thực tài khoản Belumi";

        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.User, _settings.Pass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi gửi Email: {ex.Message}");
        }
    }
}

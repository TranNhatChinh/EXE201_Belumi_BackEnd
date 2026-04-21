namespace YourApp.Infrastructure.Configuration;

public class MailSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string User { get; set; } = string.Empty;
    public string Pass { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ClientUrl { get; set; } = string.Empty;
}

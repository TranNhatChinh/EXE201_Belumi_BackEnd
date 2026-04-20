namespace YourApp.Application.Features.Auth.Refresh
{
    public class RefreshResponseDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}

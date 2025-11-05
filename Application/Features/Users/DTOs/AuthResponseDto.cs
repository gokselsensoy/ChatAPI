namespace Application.Features.Users.DTOs
{
    public class AuthResponseDto
    {
        public UserDto UserProfile { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}

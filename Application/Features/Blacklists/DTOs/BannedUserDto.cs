namespace Application.Features.Blacklists.DTOs
{
    public class BannedUserDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Reason { get; set; }
        public DateTime? FinishTime { get; set; }
        public bool IsActive { get; set; }
    }
}

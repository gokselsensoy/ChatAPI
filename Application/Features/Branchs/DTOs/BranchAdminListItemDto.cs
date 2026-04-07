namespace Application.Features.Branchs.DTOs
{
    public class BranchAdminListItemDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsBrandOwner { get; set; }
        public bool IsDelegatedAdmin { get; set; }
    }
}

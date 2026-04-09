namespace Application.Features.Users.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public Guid IdentityId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? FileId { get; set; }
        public Guid? BranchId { get; set; }
        public string Email { get; set; }
        public bool IsAnyBrandOwner { get; set; }
        public bool IsAdminAtCheckedInBranch { get; set; }
    }
}

namespace Application.Abstractions.Services
{
    public interface IIdentityService
    {
        Task UpdateUserNameAsync(Guid userId, string newUserName);
        Task<bool> IsUserNameUniqueAsync(string userName, Guid currentUserId);
    }
}

namespace Application.Abstractions.QueryRepositories
{
    public interface IBlacklistQueryRepository
    {
        Task<bool> IsUserBannedAsync(Guid userId, Guid branchId, CancellationToken cancellationToken = default);
    }
}

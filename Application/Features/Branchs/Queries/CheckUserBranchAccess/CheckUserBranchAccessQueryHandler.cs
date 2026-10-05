using Application.Abstractions.QueryRepositories;
using MediatR;

namespace Application.Features.Branches.Queries.CheckUserBranchAccess
{
    public class CheckUserBranchAccessQueryHandler : IRequestHandler<CheckUserBranchAccessQuery, bool>
    {
        private readonly IUserLocationQueryRepository _userLocationQueryRepository;
        private readonly IBranchQueryRepository _branchQueryRepository;

        // Hub'ı yoran 2 repository artık buraya ait
        public CheckUserBranchAccessQueryHandler(
            IUserLocationQueryRepository userLocationQueryRepository,
            IBranchQueryRepository branchQueryRepository)
        {
            _userLocationQueryRepository = userLocationQueryRepository;
            _branchQueryRepository = branchQueryRepository;
        }

        public async Task<bool> Handle(CheckUserBranchAccessQuery request, CancellationToken cancellationToken)
        {
            // KURAL 1: Kullanıcının aktif check-in lokasyonu istenilen şubede mi?
            var location = await _userLocationQueryRepository.GetActiveLocationByUserIdAsync(request.UserId, cancellationToken);
            if (location != null && location.BranchId == request.BranchId)
                return true;

            // KURAL 2: Kullanıcı şube yöneticisi mi? (Check-in'i olmasa bile admin olarak girebilir)
            return await _branchQueryRepository.CanUserManageBranchAsync(request.UserId, request.BranchId, cancellationToken);
        }
    }
}
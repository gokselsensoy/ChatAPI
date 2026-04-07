using Application.Abstractions.QueryRepositories;
using Domain.Entities;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Blacklists.Commands.LiftBan
{
    public class LiftBanCommandHandler : IRequestHandler<LiftBanCommand, bool>
    {
        private readonly IRepository<Blacklist> _blacklistRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBranchQueryRepository _branchQueryRepository;

        public LiftBanCommandHandler(
            IRepository<Blacklist> blacklistRepo,
            IUnitOfWork unitOfWork,
            IBranchQueryRepository branchQueryRepository)
        {
            _blacklistRepo = blacklistRepo;
            _unitOfWork = unitOfWork;
            _branchQueryRepository = branchQueryRepository;
        }

        public async Task<bool> Handle(LiftBanCommand request, CancellationToken cancellationToken)
        {
            if (!await _branchQueryRepository.CanUserManageBranchAsync(request.ActingUserId, request.BranchId, cancellationToken))
                throw new UnauthorizedAccessException("Bu şube için yasağı kaldırma yetkiniz yok.");

            // Aktif ban kaydını bul
            var blacklist = await _blacklistRepo.GetAsync(b => b.BranchId == request.BranchId && b.UserId == request.UserId && (b.FinishTime == null || b.FinishTime > DateTime.UtcNow), cancellationToken);

            if (blacklist == null) throw new Exception("Bu kullanıcının aktif bir yasağı bulunmamaktadır.");

            // Domain metodunu çağır (Geçmiş tutmak yerine _blacklistRepo.Remove(blacklist) de yapabilirsin)
            blacklist.LiftBan();

            _blacklistRepo.Update(blacklist);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

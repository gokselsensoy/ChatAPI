using Domain.Entities;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Blacklists.Commands.UpdateBan
{
    public class UpdateBanCommandHandler : IRequestHandler<UpdateBanCommand, bool>
    {
        private readonly IRepository<Blacklist> _blacklistRepo;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBanCommandHandler(IRepository<Blacklist> blacklistRepo, IUnitOfWork unitOfWork)
        {
            _blacklistRepo = blacklistRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateBanCommand request, CancellationToken cancellationToken)
        {
            var blacklist = await _blacklistRepo.GetAsync(b => b.BranchId == request.BranchId && b.UserId == request.UserId && (b.FinishTime == null || b.FinishTime > DateTime.UtcNow), cancellationToken);

            if (blacklist == null) throw new Exception("Bu kullanıcının aktif bir yasağı bulunmamaktadır.");

            blacklist.UpdateFinishTime(request.NewFinishTime);

            _blacklistRepo.Update(blacklist);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

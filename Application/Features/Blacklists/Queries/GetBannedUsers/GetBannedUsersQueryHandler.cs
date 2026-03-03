using Application.Abstractions.QueryRepositories;
using Application.Features.Blacklists.DTOs;
using Domain.Entities;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Blacklists.Queries.GetBannedUsers
{
    public class GetBannedUsersQueryHandler : IRequestHandler<GetBannedUsersQuery, List<BannedUserDto>>
    {
        private readonly IRepository<Blacklist> _blacklistRepository;
        private readonly IUserQueryRepository _userQueryRepository;

        public GetBannedUsersQueryHandler(IRepository<Blacklist> blacklistRepository, IUserQueryRepository userQueryRepository)
        {
            _blacklistRepository = blacklistRepository;
            _userQueryRepository = userQueryRepository;
        }

        public async Task<List<BannedUserDto>> Handle(GetBannedUsersQuery request, CancellationToken cancellationToken)
        {
            // 1. Şubedeki tüm ban kayıtlarını çek (Aktif olanları)
            // Eğer geçmiş banları da (süresi dolmuş) görmek istersen "FinishTime" filtresini kaldırabilirsin.
            var blacklists = await _blacklistRepository.GetAllListAsync(
                b => b.BranchId == request.BranchId && (b.FinishTime == null || b.FinishTime > DateTime.UtcNow),
                cancellationToken);

            var result = new List<BannedUserDto>();

            // 2. Kullanıcı bilgileriyle birleştir (User tablosuna Include atmak yerine böyle de yapabilirsin, 
            // AutoMapper ile ProjectTo yapmak en temizi olurdu ama bu da çalışır)
            foreach (var item in blacklists)
            {
                var user = await _userQueryRepository.GetByIdAsync(item.UserId, cancellationToken);

                result.Add(new BannedUserDto
                {
                    UserId = item.UserId,
                    UserName = user?.UserName ?? "Bilinmeyen Kullanıcı",
                    Reason = item.Reason,
                    FinishTime = item.FinishTime,
                    IsActive = true
                });
            }

            return result;
        }
    }
}

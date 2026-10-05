using Domain.Repositories;
using Application.Abstractions.QueryRepositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Queries.GetSharedPeersIdentityIds
{
    public class GetSharedPeersIdentityIdsQueryHandler : IRequestHandler<GetSharedPeersIdentityIdsQuery, List<string>>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IUserQueryRepository _userQueryRepository;

        // Hub'daki yükleri buraya alıyoruz
        public GetSharedPeersIdentityIdsQueryHandler(
            IChatRoomRepository chatRoomRepository,
            IUserQueryRepository userQueryRepository)
        {
            _chatRoomRepository = chatRoomRepository;
            _userQueryRepository = userQueryRepository;
        }

        public async Task<List<string>> Handle(GetSharedPeersIdentityIdsQuery request, CancellationToken cancellationToken)
        {
            // 1. Kullanıcının ortak odalardaki arkadaşlarının User ID'lerini bul
            var peerUserIds = await _chatRoomRepository.GetSharedRoomPeerUserIdsAsync(request.UserId, cancellationToken);

            if (peerUserIds == null || peerUserIds.Count == 0)
                return new List<string>();

            // 2. Bu User ID'lerine karşılık gelen Identity ID'leri veritabanından çek (Map yapısı)
            var identityMap = await _userQueryRepository.GetIdentityIdsByUserIdsAsync(peerUserIds, cancellationToken);

            // 3. Sadece değerleri string listesi (örn: Keycloak veya IdentityUser Id'leri) olarak dön
            return identityMap.Values.Select(id => id.ToString()).ToList();
        }
    }
}
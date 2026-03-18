using Application.Abstractions.QueryRepositories;
using Application.Features.ChatRooms.DTOs;
using Domain.Entities;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRooms.Queries.GetPublicRoomsByBranch
{
    public class GetPublicRoomsByBranchQueryHandler : IRequestHandler<GetPublicRoomsByBranchQuery, List<ChatRoomDto>>
    {
        private readonly IChatRoomQueryRepository _chatRoomQueryRepository;
        // EKLENDİ: Kullanıcının anlık konumunu bulmak için
        private readonly IRepository<UserLocation> _userLocationRepository;

        public GetPublicRoomsByBranchQueryHandler(
            IChatRoomQueryRepository chatRoomQueryRepository,
            IRepository<UserLocation> userLocationRepository)
        {
            _chatRoomQueryRepository = chatRoomQueryRepository;
            _userLocationRepository = userLocationRepository;
        }

        public async Task<List<ChatRoomDto>> Handle(GetPublicRoomsByBranchQuery request, CancellationToken cancellationToken)
        {
            // 1. Kullanıcının o an hangi şubede check-in yaptığını UserLocation'dan bul
            // Not: Eğer request.UserId Token'dan gelen IdentityId ise, burada önceki mesajlardaki gibi 
            // gerçek AppUser Id'sine dönüştürme yapmayı unutma.
            var currentLocation = await _userLocationRepository.GetAsync(
                ul => ul.UserId == request.UserId,
                cancellationToken);

            // 2. Eğer kullanıcı hiçbir şubede değilse (Check-in yapmamışsa) hata fırlat
            if (currentLocation == null)
                throw new UnauthorizedAccessException("Oda listelemek için önce bir şubeye check-in yapmalısınız.");

            // 3. Kullanıcının bulunduğu şubenin odalarını getir
            return await _chatRoomQueryRepository.GetPublicRoomsByBranchIdAsync(currentLocation.BranchId, cancellationToken);
        }
    }
}

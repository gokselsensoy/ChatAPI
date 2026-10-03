using Domain.Repositories;
using Application.Abstractions.QueryRepositories;
using Application.Exceptions; // Veya NotFoundException neredeyse
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ChatRooms.Queries.CheckUserRoomAccess
{
    public class CheckUserRoomAccessQueryHandler : IRequestHandler<CheckUserRoomAccessQuery, bool>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IUserLocationQueryRepository _userLocationQueryRepository;
        private readonly IBranchQueryRepository _branchQueryRepository;

        public CheckUserRoomAccessQueryHandler(
            IChatRoomRepository chatRoomRepository,
            IUserLocationQueryRepository userLocationQueryRepository,
            IBranchQueryRepository branchQueryRepository)
        {
            _chatRoomRepository = chatRoomRepository;
            _userLocationQueryRepository = userLocationQueryRepository;
            _branchQueryRepository = branchQueryRepository;
        }

        public async Task<bool> Handle(CheckUserRoomAccessQuery request, CancellationToken cancellationToken)
        {
            var room = await _chatRoomRepository.GetByIdAsync(request.RoomId, cancellationToken);

            if (room == null) throw new NotFoundException("Oda bulunamadı.");

            if (room.IsMemberOnlyRoom) return room.ChatRoomUserMaps.Any(m => m.UserId == request.UserId);

            var location = await _userLocationQueryRepository.GetActiveLocationByUserIdAsync(request.UserId, cancellationToken);

            if (location != null && location.BranchId == room.BranchId) return true;

            return await _branchQueryRepository.CanUserManageBranchAsync(request.UserId, room.BranchId, cancellationToken);
        }
    }
}
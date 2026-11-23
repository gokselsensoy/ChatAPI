using Application.Exceptions;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRooms.Commands.ToggleChatMute
{
    public class ToggleChatMuteCommandHandler : IRequestHandler<ToggleChatMuteCommand, bool>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ToggleChatMuteCommandHandler(
            IChatRoomRepository chatRoomRepository,
            IUnitOfWork unitOfWork)
        {
            _chatRoomRepository = chatRoomRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ToggleChatMuteCommand request, CancellationToken cancellationToken)
        {
            var room = await _chatRoomRepository.GetByIdWithUsersAsync(request.RoomId, cancellationToken);
            if (room == null || room.IsDeleted)
                throw new NotFoundException(nameof(ChatRoom), request.RoomId);

            var userMap = await _chatRoomRepository.GetUserMapAsync(request.RoomId, request.UserId, cancellationToken);
            if (userMap == null)
                throw new UnauthorizedAccessException("Bu sohbette üyeliğiniz bulunamadı.");

            userMap.SetMuted(request.IsMuted);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return userMap.IsMuted;
        }
    }
}

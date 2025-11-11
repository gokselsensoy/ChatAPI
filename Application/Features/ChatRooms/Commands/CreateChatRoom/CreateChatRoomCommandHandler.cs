using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRooms.Commands.CreateChatRoom
{
    public class CreateChatRoomCommandHandler : IRequestHandler<CreateChatRoomCommand, Guid>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IUnitOfWork _unitOfWork;
        // Odayı oluşturanı odaya otomatik eklemek için IChatRoomUserMapRepository de eklenebilir

        public CreateChatRoomCommandHandler(IChatRoomRepository chatRoomRepository, IUnitOfWork unitOfWork)
        {
            _chatRoomRepository = chatRoomRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateChatRoomCommand request, CancellationToken cancellationToken)
        {
            var chatRoom = ChatRoom.Create(
                request.Name,
                request.BranchId,
                request.RoomType
            );

            _chatRoomRepository.Add(chatRoom);

            // TODO: Odayı oluşturan kişiyi odaya otomatik ekle (Feature 3)
            // (Ayrı bir 'JoinChatRoomCommand' göndermek daha temiz olabilir)

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return chatRoom.Id;
        }
    }
}

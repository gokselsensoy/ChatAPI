using Application.Features.ChatRoomInvites.DTOs;
using Domain.Enums;
using Domain.Repositories;
using MediatR;

namespace Application.Features.ChatRoomInvites.Queries.GetIncomingChatRoomInvites
{
    public class GetIncomingChatRoomInvitesQueryHandler
        : IRequestHandler<GetIncomingChatRoomInvitesQuery, List<IncomingChatRoomInviteDto>>
    {
        private const string DefaultGroupName = "Grup Sohbeti";

        private readonly IChatRoomInviteRepository _inviteRepository;

        public GetIncomingChatRoomInvitesQueryHandler(IChatRoomInviteRepository inviteRepository)
        {
            _inviteRepository = inviteRepository;
        }

        public async Task<List<IncomingChatRoomInviteDto>> Handle(
            GetIncomingChatRoomInvitesQuery request,
            CancellationToken cancellationToken)
        {
            var invites = await _inviteRepository.GetPendingIncomingWithDetailsAsync(
                request.InviteeUserId,
                cancellationToken);

            return invites.Select(invite =>
            {
                var sourceRoom = invite.ChatRoom;
                var isGroup = invite.TargetRoomType == RoomType.Group;

                string? groupName = null;
                if (isGroup)
                {
                    groupName = sourceRoom?.RoomType == RoomType.Group
                        ? sourceRoom.Name
                        : DefaultGroupName;
                }

                return new IncomingChatRoomInviteDto
                {
                    InviteId = invite.Id,
                    Status = invite.Status.ToString(),
                    CreatedDate = invite.CreatedDate,
                    TargetRoomType = invite.TargetRoomType.ToString(),
                    InviterUserId = invite.InviterUserId,
                    InviterUserName = invite.InviterUser?.UserName ?? "Kullanıcı",
                    InviterFirstName = invite.InviterUser?.FirstName ?? string.Empty,
                    InviterLastName = invite.InviterUser?.LastName ?? string.Empty,
                    InviterFileId = invite.InviterUser?.FileId,
                    BranchId = sourceRoom?.BranchId ?? Guid.Empty,
                    BranchName = sourceRoom?.Branch?.Name ?? string.Empty,
                    SourceChatRoomId = invite.ChatRoomId,
                    SourceChatRoomName = sourceRoom?.Name ?? string.Empty,
                    GroupName = groupName
                };
            }).ToList();
        }
    }
}

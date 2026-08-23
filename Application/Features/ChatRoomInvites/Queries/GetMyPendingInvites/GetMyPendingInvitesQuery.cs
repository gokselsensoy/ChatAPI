using Application.Features.ChatRoomInvites.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.ChatRoomInvites.Queries.GetMyPendingInvites
{
    public class GetMyPendingInvitesQuery : IRequest<List<ChatRoomInviteDto>>
    {
        public Guid UserId { get; set; }
    }
}

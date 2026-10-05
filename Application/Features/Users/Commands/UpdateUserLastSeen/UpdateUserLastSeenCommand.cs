using Application.Abstractions.Messaging;
using System;

namespace Application.Features.Users.Commands.UpdateUserLastSeen
{
    public class UpdateUserLastSeenCommand : ICommand<DateTime?>
    {
        public Guid UserId { get; set; }
    }
}
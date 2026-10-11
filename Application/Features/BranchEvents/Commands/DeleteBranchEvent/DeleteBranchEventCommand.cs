using MediatR;
using System;

namespace Application.Features.BranchEvents.Commands.DeleteBranchEvent
{
    public class DeleteBranchEventCommand : IRequest<bool>
    {
        public Guid EventId { get; set; }
    }
}

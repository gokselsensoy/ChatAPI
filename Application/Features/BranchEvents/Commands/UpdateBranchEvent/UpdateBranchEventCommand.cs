using MediatR;
using System;

namespace Application.Features.BranchEvents.Commands.UpdateBranchEvent
{
    public class UpdateBranchEventCommand : IRequest<bool>
    {
        public Guid EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsHighlighted { get; set; }
        public string? ImageUrl { get; set; }
    }
}
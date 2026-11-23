using Domain.Enums;
using MediatR;

namespace Application.Features.Users.Commands.ReportUser
{
    public class ReportUserCommand : IRequest<Guid>
    {
        public Guid ReporterUserId { get; set; }
        public Guid ReportedUserId { get; set; }
        public Guid? ChatRoomId { get; set; }
        public Guid? MessageId { get; set; }
        public ReportReason Reason { get; set; }
        public string? Description { get; set; }
    }
}

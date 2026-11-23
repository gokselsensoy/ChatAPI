using Application.Abstractions.QueryRepositories;
using Application.Exceptions;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Users.Commands.ReportUser
{
    public class ReportUserCommandHandler : IRequestHandler<ReportUserCommand, Guid>
    {
        private readonly IUserReportRepository _userReportRepository;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReportUserCommandHandler(
            IUserReportRepository userReportRepository,
            IUserQueryRepository userQueryRepository,
            IUnitOfWork unitOfWork)
        {
            _userReportRepository = userReportRepository;
            _userQueryRepository = userQueryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(ReportUserCommand request, CancellationToken cancellationToken)
        {
            var reportedUser = await _userQueryRepository.GetByIdAsync(request.ReportedUserId, cancellationToken);
            if (reportedUser == null)
                throw new NotFoundException(nameof(User), request.ReportedUserId);

            var report = UserReport.Create(
                request.ReporterUserId,
                request.ReportedUserId,
                request.Reason,
                request.Description,
                request.ChatRoomId,
                request.MessageId);

            _userReportRepository.Add(report);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return report.Id;
        }
    }
}

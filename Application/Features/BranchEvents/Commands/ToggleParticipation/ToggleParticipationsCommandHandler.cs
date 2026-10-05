using Domain.Repositories;
using Domain.SeedWork;
using Domain.Entities;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Exceptions; // NotFoundException'ın bulunduğu namespace

namespace Application.Features.BranchEvents.Commands.ToggleParticipation
{
    public class ToggleEventParticipationCommandHandler : IRequestHandler<ToggleEventParticipationCommand, bool>
    {
        private readonly IBranchEventRepository _eventRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ToggleEventParticipationCommandHandler(IBranchEventRepository eventRepository, IUnitOfWork unitOfWork)
        {
            _eventRepository = eventRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ToggleEventParticipationCommand request, CancellationToken cancellationToken)
        {
            var branchEvent = await _eventRepository.GetByIdWithParticipantsAsync(request.EventId, cancellationToken);
            if (branchEvent == null) throw new NotFoundException("Etkinlik bulunamadı.");

            var participant = branchEvent.Participants.FirstOrDefault(p => p.UserId == request.UserId);

            if (participant == null)
            {
                // İlk kez basıyorsa kaydı oluştur ve ekle (Varsayılan olarak IsAttending = true gelir)
                participant = BranchEventParticipant.Create(branchEvent.Id, request.UserId);
                branchEvent.Participants.Add(participant);
            }
            else
            {
                // Daha önce tıklamışsa durumu tersine çevir (Katılımı iptal et veya tekrar katıl)
                participant.ToggleParticipation();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return participant.IsAttending;
        }
    }
}
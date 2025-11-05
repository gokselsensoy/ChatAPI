using Application.Exceptions;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Users.Commands.UpdateMyProfile
{
    public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateMyProfileCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
        {
            // 1. Kullanıcıyı lokal ID'sine göre bul
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
                throw new NotFoundException(nameof(User), request.UserId);

            // 2. Güvenlik doğrulaması (Token'daki IdentityId, bulunan kaydınkiyle eşleşmeli)
            if (user.IdentityId != request.IdentityId)
                throw new UnauthorizedAccessException("Bu profili güncelleme yetkiniz yok.");

            // 3. Domain metodunu çağır
            user.UpdateProfile(request.FirstName, request.LastName, request.FileId);

            // 4. Değişiklikleri kaydet (Bu, 'UserProfileUpdatedDomainEvent'i tetikler)
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

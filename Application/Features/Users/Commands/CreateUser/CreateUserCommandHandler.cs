using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // Bu IdentityId'ye sahip profil zaten var mı?
            var existingUser = await _userRepository.GetByIdentityIdAsync(request.IdentityId, cancellationToken);
            if (existingUser != null)
                return existingUser.Id; // Zaten var

            // Domain (User) Profilimizi Oluştur
            var userProfile = User.Create(
                request.IdentityId,
                request.UserName,
                request.FirstName,
                request.LastName,
                request.UserType
            );

            _userRepository.Add(userProfile);

            // TransactionPipeline 
            // burada devreye girmese de (çünkü Controller'da transaction'ı başlattık),
            // sadece SaveChanges dememiz yeterli.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return userProfile.Id;
        }
    }
}

using Application.Abstractions.Services;
using Domain.Events.UserEvents;
using Domain.Repositories;
using MediatR;

namespace Application.Features.Users.EventHandlers
{
    public class UserProfileUpdatedDomainEventHandler : INotificationHandler<UserProfileUpdatedDomainEvent>
    {
        private readonly IIdentityService _identityService;

        public UserProfileUpdatedDomainEventHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task Handle(UserProfileUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            // Artık Identity detaylarını bilmesine gerek yok, sadece emri verir.
            await _identityService.UpdateUserNameAsync(notification.IdentityId, notification.NewUserName);
        }
    }
}

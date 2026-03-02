using Application.Abstractions.Messaging;
using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Features.Users.Queries.GetMyProfile
{
    public class GetMyProfileQuery : IRequest<UserDto>
    {
        public Guid IdentityId { get; set; }
    }
}

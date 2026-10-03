using Application.Features.Users.DTOs;
using MediatR;
using System;

namespace Application.Features.Users.Queries.GetUserByIdentityId
{
    public class GetUserByIdentityIdQuery : IRequest<UserDto?>
    {
        public Guid IdentityId { get; set; }
    }
}
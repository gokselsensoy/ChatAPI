using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.Users.Queries.GetSharedPeersIdentityIds
{
    // İşlem sonucunda Identity Id'leri içeren bir string listesi dönecek
    public class GetSharedPeersIdentityIdsQuery : IRequest<List<string>>
    {
        public Guid UserId { get; set; }
    }
}
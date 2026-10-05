using Application.Features.BranchEvents.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.BranchEvents.Queries.GetBranchEvents
{
    public class GetBranchEventsQuery : IRequest<List<BranchEventDto>>
    {
        public Guid BranchId { get; set; }
        public Guid UserId { get; set; } // Kullanıcının o etkinliğe özel butonlarını belirlemek için şart
    }
}
using Application.Features.Menus.DTOs;
using MediatR;

namespace Application.Features.Menus.Queries.GetBrandOwnerMenusGrouped
{
    public class GetBrandOwnerMenusGroupedQuery : IRequest<List<BranchMenusGroupDto>>
    {
        /// <summary>Domain User.Id (controller doldurur).</summary>
        public Guid ActingUserId { get; set; }
    }
}

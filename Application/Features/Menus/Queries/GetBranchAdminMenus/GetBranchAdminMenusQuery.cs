using Application.Features.Menus.DTOs;
using MediatR;

namespace Application.Features.Menus.Queries.GetBranchAdminMenus
{
    public class GetBranchAdminMenusQuery : IRequest<List<MenuDto>>
    {
        public Guid BranchId { get; set; }

        /// <summary>Domain User.Id (controller doldurur).</summary>
        public Guid ActingUserId { get; set; }
    }
}

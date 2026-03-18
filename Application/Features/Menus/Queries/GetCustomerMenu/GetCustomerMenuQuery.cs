using MediatR;
using Application.Features.Menus.DTOs;

namespace Application.Features.Menus.Queries.GetCustomerMenu
{
    public class GetCustomerMenuQuery : IRequest<List<MenuDto>>
    {
        public Guid BranchId { get; set; }
        public Guid UserId { get; set; }
    }
}

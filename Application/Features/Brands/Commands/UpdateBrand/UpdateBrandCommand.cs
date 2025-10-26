using Application.Abstractions.Messaging;

namespace Application.Features.Brands.Commands.UpdateBrand
{
    public class UpdateBrandOwnerCommand : ICommand
    {
        public Guid BrandId { get; set; }
        public Guid NewOwnerUserId { get; set; }
    }
}

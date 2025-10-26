using Application.Abstractions.Messaging;

namespace Application.Features.Brands.Commands.DeleteBrand
{
    public class DeleteBrandCommand : ICommand
    {
        public Guid BrandId { get; set; }
    }
}

using Application.Abstractions.Messaging;

namespace Application.Features.Brands.Commands.CreateBrand
{
    public class CreateBrandCommand : ICommand<Guid>
    {
        public string Name { get; set; }
        public Guid OwnerUserId { get; set; }
        public string? FileId { get; set; }
    }
}

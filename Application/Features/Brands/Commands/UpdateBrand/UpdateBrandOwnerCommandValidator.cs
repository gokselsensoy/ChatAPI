using FluentValidation;

namespace Application.Features.Brands.Commands.UpdateBrand
{
    public class UpdateBrandOwnerCommandValidator : AbstractValidator<UpdateBrandOwnerCommand>
    {
        public UpdateBrandOwnerCommandValidator()
        {
            RuleFor(x => x.BrandId).NotEmpty();
            RuleFor(x => x.NewOwnerUserId).NotEmpty();
        }
    }
}

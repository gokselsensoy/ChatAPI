using FluentValidation;

namespace Application.Features.Branchs.Commands
{
    public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
    {
        public CreateBranchCommandValidator()
        {
            RuleFor(x => x.BrandId).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MinimumLength(2);

            // Adres Zorunlulukları
            RuleFor(x => x.Country).NotEmpty();
            RuleFor(x => x.City).NotEmpty();
            RuleFor(x => x.District).NotEmpty();
            RuleFor(x => x.Neighborhood).NotEmpty();
            RuleFor(x => x.Street).NotEmpty();
            RuleFor(x => x.BuildingNumber).NotEmpty();
            RuleFor(x => x.ZipCode).NotEmpty();

            // Lat/Long zorunlu ve geçerli aralıkta olmalı
            RuleFor(x => x.Latitude)
                .NotEmpty()
                .InclusiveBetween(-90, 90).WithMessage("Latitude -90 ile 90 arasında olmalıdır.");

            RuleFor(x => x.Longitude)
                .NotEmpty()
                .InclusiveBetween(-180, 180).WithMessage("Longitude -180 ile 180 arasında olmalıdır.");
        }
    }
}

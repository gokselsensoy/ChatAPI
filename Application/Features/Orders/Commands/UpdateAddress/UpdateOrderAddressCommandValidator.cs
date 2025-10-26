using Application.Features.Orders.Commands.CreateOrder;
using FluentValidation;

namespace Application.Features.Orders.Commands.UpdateAddress
{
    public class UpdateOrderAddressCommandValidator : AbstractValidator<UpdateOrderAddressCommand>
    {
        public UpdateOrderAddressCommandValidator()
        {
            RuleFor(x => x.ZipCode).NotEmpty().WithMessage("Zip code boş olamaz");
            RuleFor(x => x.City).NotEmpty().MinimumLength(3).WithMessage("Şehir alanı gereklidir.");
            RuleFor(x => x.Street).NotEmpty().WithMessage("Sokak alanı gereklidir.");
        }
    }
}

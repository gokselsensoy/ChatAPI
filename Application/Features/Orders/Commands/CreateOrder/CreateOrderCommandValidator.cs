using FluentValidation;

namespace Application.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Müşteri ID'si boş olamaz.");
            RuleFor(x => x.City).NotEmpty().MinimumLength(3).WithMessage("Şehir alanı gereklidir.");
            RuleFor(x => x.Street).NotEmpty().WithMessage("Sokak alanı gereklidir.");
            RuleFor(x => x.OrderItems).NotEmpty().WithMessage("Sipariş en az bir ürün içermelidir.");

            RuleForEach(x => x.OrderItems).ChildRules(item =>
            {
                item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");
                item.RuleFor(i => i.ProductId).NotEmpty();
            });
        }
    }
}

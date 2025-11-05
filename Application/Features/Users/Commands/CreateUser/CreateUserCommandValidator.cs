using FluentValidation;

namespace Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            // Bu komut, Identity kullanıcısının (AspNetUsers)
            // ZATEN oluşturulduğunu varsayar.

            RuleFor(x => x.IdentityId)
                .NotEmpty().WithMessage("IdentityId (AspNetUsers.Id) boş olamaz.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Kullanıcı adı boş olamaz.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email boş olamaz.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("İsim (FirstName) boş olamaz.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Soyisim (LastName) boş olamaz.");
        }
    }
}

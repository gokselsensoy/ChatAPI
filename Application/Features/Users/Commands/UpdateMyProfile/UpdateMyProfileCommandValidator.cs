using FluentValidation;

namespace Application.Features.Users.Commands.UpdateMyProfile
{
    public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
    {
        public UpdateMyProfileCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.IdentityId).NotEmpty();

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("İsim boş olamaz.")
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Soyisim boş olamaz.")
                .MaximumLength(100);
        }
    }
}

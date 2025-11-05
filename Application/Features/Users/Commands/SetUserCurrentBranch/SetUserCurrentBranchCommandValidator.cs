using FluentValidation;

namespace Application.Features.Users.Commands.SetUserCurrentBranch
{
    public class SetUserCurrentBranchCommandValidator : AbstractValidator<SetUserCurrentBranchCommand>
    {
        public SetUserCurrentBranchCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Kullanıcı ID'si boş olamaz.");
        }
    }
}

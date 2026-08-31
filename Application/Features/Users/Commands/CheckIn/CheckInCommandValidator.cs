using FluentValidation;

namespace Application.Features.Users.Commands.CheckIn
{
    public class CheckInCommandValidator : AbstractValidator<CheckInCommand>
    {
        public CheckInCommandValidator()
        {
            RuleFor(x => x.BranchId)
                .NotEmpty().WithMessage("Şube (BranchId) boş olamaz.");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude -90 ile 90 arasında olmalıdır.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude -180 ile 180 arasında olmalıdır.");
        }
    }
}

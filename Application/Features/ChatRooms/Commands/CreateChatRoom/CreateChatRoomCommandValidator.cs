using FluentValidation;

namespace Application.Features.ChatRooms.Commands.CreateChatRoom
{
    public class CreateChatRoomCommandValidator : AbstractValidator<CreateChatRoomCommand>
    {
        public CreateChatRoomCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.RoomType).IsInEnum();
            RuleFor(x => x.BranchId).NotEmpty();
        }
    }
}

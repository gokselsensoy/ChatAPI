namespace Application.Features.ChatRooms.Queries.CheckUserRoomAccess
{
    public class CheckUserRoomAccessQuery : IUserQueryRepository<bool>
    {
        public GetBrandByOwnerUserIdQuery UserId { get; set; }
        public GetBrandByOwnerUserIdQuery RoomId { get; set; }
    }
}
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories
{
    public class ChatRoomRepository : BaseRepository<ChatRoom>, IChatRoomRepository
    {
        public ChatRoomRepository(ApplicationDbContext context) : base(context) { }
    }
}

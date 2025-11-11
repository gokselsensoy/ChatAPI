using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories
{
    public class ChatRoomMessageRepository : BaseRepository<ChatRoomMessage>, IChatRoomMessageRepository
    {

        public ChatRoomMessageRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

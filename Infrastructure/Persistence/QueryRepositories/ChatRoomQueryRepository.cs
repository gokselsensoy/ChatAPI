using Application.Abstractions.QueryRepositories;
using Application.Features.ChatRooms.DTOs;
using Application.Shared.Pagination;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.QueryRepositories
{
    public class ChatRoomQueryRepository : IChatRoomQueryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ChatRoomQueryRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ChatRoomDto>> GetPublicRoomsByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default)
        {
            return await _context.ChatRooms
                .AsNoTracking()
                .Where(cr => cr.BranchId == branchId && cr.RoomType == RoomType.Public)
                .OrderBy(cr => cr.Name)
                .ProjectTo<ChatRoomDto>(_mapper.ConfigurationProvider, cancellationToken)
                .ToListAsync(cancellationToken);
        }

        public async Task<PaginatedResponse<ChatRoomMessageDto>> GetMessagesForRoomAsync(
            Guid roomId,
            PaginatedRequest pagination,
            CancellationToken cancellationToken = default)
        {
            var query = _context.ChatRoomMessages
                .AsNoTracking()
                .Where(m => m.ChatRoomId == roomId)
                .Include(m => m.SenderUser)
                .OrderByDescending(m => m.CreatedDate)
                .ProjectTo<ChatRoomMessageDto>(_mapper.ConfigurationProvider, cancellationToken);

            var count = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResponse<ChatRoomMessageDto>(items, count, pagination.PageNumber, pagination.PageSize);
        }
    }
}

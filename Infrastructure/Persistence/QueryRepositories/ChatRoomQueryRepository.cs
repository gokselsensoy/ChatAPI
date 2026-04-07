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
        private readonly IBranchQueryRepository _branchQueryRepository;

        public ChatRoomQueryRepository(
            ApplicationDbContext context,
            IMapper mapper,
            IBranchQueryRepository branchQueryRepository)
        {
            _context = context;
            _mapper = mapper;
            _branchQueryRepository = branchQueryRepository;
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
            Guid branchId,
            RoomType roomType,
            PaginatedRequest pagination,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var query = _context.ChatRoomMessages
                            .AsNoTracking()
                            .Where(m => m.ChatRoomId == roomId);

            if (roomType == RoomType.Public)
            {
                query = query.Where(m => m.CreatedDate >= DateTime.UtcNow.AddHours(-2));
            }

            var count = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(m => m.CreatedDate)
                .ProjectTo<ChatRoomMessageDto>(_mapper.ConfigurationProvider, cancellationToken)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            var adminTagUserIds = await _branchQueryRepository.GetBranchPrivilegedUserIdsAsync(branchId, cancellationToken);

            foreach (var item in items)
            {
                item.IsMine = item.SenderUserId == currentUserId;
                item.SenderRole = adminTagUserIds.Contains(item.SenderUserId) ? "Admin" : "Müşteri";
            }

            return new PaginatedResponse<ChatRoomMessageDto>(items, count, pagination.PageNumber, pagination.PageSize);
        }
    }
}

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

        public async Task<List<ChatRoomDto>> GetPublicRoomsByBranchIdAsync(
            Guid branchId,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var rooms = await _context.ChatRooms
                .AsNoTracking()
                .Where(cr => cr.BranchId == branchId && cr.RoomType == RoomType.Public && !cr.IsDeleted)
                .OrderBy(cr => cr.Name)
                .Select(cr => new
                {
                    Room = cr,
                    MemberCount = cr.ChatRoomUserMaps.Count,
                    LastMessage = cr.Messages
                        .Where(m => m.CreatedDate >= DateTime.UtcNow.AddHours(-2))
                        .OrderByDescending(m => m.CreatedDate)
                        .Select(m => new { m.Message, m.CreatedDate, m.SenderUserId })
                        .FirstOrDefault(),
                    LastReadAt = cr.ChatRoomUserMaps
                        .Where(m => m.UserId == currentUserId)
                        .Select(m => m.LastReadAt)
                        .FirstOrDefault(),
                    UnreadCount = cr.ChatRoomUserMaps.Any(m => m.UserId == currentUserId)
                        ? cr.Messages.Count(msg =>
                            msg.CreatedDate >= DateTime.UtcNow.AddHours(-2)
                            && msg.CreatedDate > (cr.ChatRoomUserMaps
                                .Where(m => m.UserId == currentUserId)
                                .Select(m => m.LastReadAt)
                                .FirstOrDefault() ?? DateTime.MinValue)
                            && msg.SenderUserId != currentUserId)
                        : cr.Messages.Count(msg =>
                            msg.CreatedDate >= DateTime.UtcNow.AddHours(-2)
                            && msg.SenderUserId != currentUserId)
                })
                .ToListAsync(cancellationToken);

            return rooms.Select(x =>
            {
                var lastAt = x.LastMessage?.CreatedDate;
                var lastRead = x.LastReadAt;
                var hasNew = lastAt.HasValue && (!lastRead.HasValue || lastAt > lastRead);

                return new ChatRoomDto
                {
                    Id = x.Room.Id,
                    Name = x.Room.Name,
                    RoomType = x.Room.RoomType.ToString(),
                    BranchId = x.Room.BranchId,
                    MemberCount = x.MemberCount,
                    LastMessagePreview = TruncatePreview(x.LastMessage?.Message),
                    LastMessageAt = lastAt,
                    LastMessageSenderUserId = x.LastMessage?.SenderUserId,
                    HasNew = hasNew,
                    UnreadCount = x.UnreadCount
                };
            }).ToList();
        }

        public async Task<List<ChatRoomDto>> GetPrivateInboxAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var rooms = await _context.ChatRooms
                .AsNoTracking()
                .Where(cr =>
                    !cr.IsDeleted
                    && (cr.RoomType == RoomType.Private || cr.RoomType == RoomType.Group)
                    && cr.ChatRoomUserMaps.Any(m => m.UserId == userId))
                .Select(cr => new
                {
                    Room = cr,
                    MemberCount = cr.ChatRoomUserMaps.Count,
                    LastMessage = cr.Messages
                        .OrderByDescending(m => m.CreatedDate)
                        .Select(m => new { m.Message, m.CreatedDate, m.SenderUserId })
                        .FirstOrDefault(),
                    LastReadAt = cr.ChatRoomUserMaps
                        .Where(m => m.UserId == userId)
                        .Select(m => m.LastReadAt)
                        .FirstOrDefault(),
                    UnreadCount = cr.Messages.Count(msg =>
                        msg.SenderUserId != userId
                        && msg.CreatedDate > (cr.ChatRoomUserMaps
                            .Where(m => m.UserId == userId)
                            .Select(m => m.LastReadAt)
                            .FirstOrDefault() ?? DateTime.MinValue))
                })
                .ToListAsync(cancellationToken);

            return rooms
                .Select(x =>
                {
                    var lastAt = x.LastMessage?.CreatedDate;
                    var lastRead = x.LastReadAt;
                    var hasNew = lastAt.HasValue && (!lastRead.HasValue || lastAt > lastRead);

                    return new ChatRoomDto
                    {
                        Id = x.Room.Id,
                        Name = x.Room.Name,
                        RoomType = x.Room.RoomType.ToString(),
                        BranchId = x.Room.BranchId,
                        MemberCount = x.MemberCount,
                        LastMessagePreview = TruncatePreview(x.LastMessage?.Message),
                        LastMessageAt = lastAt,
                        LastMessageSenderUserId = x.LastMessage?.SenderUserId,
                        HasNew = hasNew,
                        UnreadCount = x.UnreadCount
                    };
                })
                .OrderByDescending(r => r.LastMessageAt ?? DateTime.MinValue)
                .ToList();
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

        private static string? TruncatePreview(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            const int maxLen = 120;
            return message.Length <= maxLen ? message : message[..maxLen] + "...";
        }
    }
}

using Application.Abstractions.QueryRepositories;
using Application.Features.ChatRoomInvites.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.QueryRepositories
{
    public class ChatRoomInviteQueryRepository : IChatRoomInviteQueryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ChatRoomInviteQueryRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ChatRoomInviteDto>> GetPendingInvitesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.ChatRoomInvites
                .AsNoTracking()
                .Where(i => i.InviteeUserId == userId && i.Status == InviteStatus.Pending)
                .OrderByDescending(i => i.CreatedDate)
                .ProjectTo<ChatRoomInviteDto>(_mapper.ConfigurationProvider, cancellationToken)
                .ToListAsync(cancellationToken);
        }
    }
}

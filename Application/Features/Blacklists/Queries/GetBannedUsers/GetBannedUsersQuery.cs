using Application.Features.Blacklists.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Blacklists.Queries.GetBannedUsers
{
    public class GetBannedUsersQuery : IRequest<List<BannedUserDto>>
    {
        public Guid BranchId { get; set; }
    }
}

using Application.Abstractions.Messaging;
using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;
using MediatR;

namespace Application.Features.Branchs.Queries.GetNearbyBranches
{
    public class GetNearbyBranchesQuery : PaginatedRequest, IRequest<PaginatedResponse<NearbyBranchDto>>
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int RadiusInMeters { get; set; } = 5000; // Varsayılan 5km
    }
}

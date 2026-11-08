using Application.Shared.Pagination;
using System.Collections.Generic;

namespace Application.Features.Branchs.DTOs
{
    public class NearbyBranchesResponse : PaginatedResponse<NearbyBranchDto>
    {
        public int CheckInAvailableCount { get; init; }
        public int CheckInRadiusInMeters { get; init; }

        public NearbyBranchesResponse(
            List<NearbyBranchDto> items,
            int count,
            int pageNumber,
            int pageSize,
            int checkInAvailableCount,
            int checkInRadiusInMeters)
            : base(items, count, pageNumber, pageSize)
        {
            CheckInAvailableCount = checkInAvailableCount;
            CheckInRadiusInMeters = checkInRadiusInMeters;
        }
    }
}

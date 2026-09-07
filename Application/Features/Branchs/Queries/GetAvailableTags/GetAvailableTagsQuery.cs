using Domain;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Branchs.Queries.GetAvailableTags
{
    public class GetAvailableTagsQuery : IRequest<List<string>>
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int DistanceInMeters { get; set; } = GeoConstants.NearbyBranchesDefaultRadiusInMeters;
        public Guid? CurrentUserId { get; set; }
    }
}

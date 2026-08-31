using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;
using Domain;
using MediatR;

namespace Application.Features.Branchs.Queries.GetNearbyBranches
{
    public class GetNearbyBranchesQuery : PaginatedRequest, IRequest<PaginatedResponse<NearbyBranchDto>>
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int RadiusInMeters { get; set; } = GeoConstants.NearbyBranchesDefaultRadiusInMeters;

        /// <summary>
        /// Şube etiketlerine göre filtre. Null, boş veya gönderilmezse tüm şubeler döner.
        /// Örnek: Tags=Kahve&amp;Tags=Canlı Müzik
        /// </summary>
        public List<string>? Tags { get; set; }

        public Guid? CurrentUserId { get; set; }
    }
}

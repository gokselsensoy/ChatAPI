using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;
using Domain;
using Domain.Enums;
using MediatR;

namespace Application.Features.Branchs.Queries.GetNearbyBranches
{
    public class GetNearbyBranchesQuery : PaginatedRequest, IRequest<NearbyBranchesResponse>
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int RadiusInMeters { get; set; } = GeoConstants.NearbyBranchesDefaultRadiusInMeters;

        /// <summary>
        /// Şube etiketlerine göre filtre (VE / AND mantığı). Seçilen tüm etiketleri içeren şubeler döner.
        /// Null, boş veya gönderilmezse tüm şubeler döner.
        /// Örnek: Tags=Kahve&amp;Tags=CanliMuzik
        /// </summary>
        public List<BranchTag>? Tags { get; set; }

        public Guid? CurrentUserId { get; set; }
    }
}

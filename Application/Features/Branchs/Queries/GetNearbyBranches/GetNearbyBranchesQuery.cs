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

        /// <summary>
        /// Mekan tipine göre filtre (Pub, Cafe). Null ise tüm tipler döner.
        /// </summary>
        public BranchType? BranchType { get; set; }

        /// <summary>
        /// True ise sadece kullanıcının check-in yarıçapı (100m) içindeki şubeler döner.
        /// </summary>
        public bool OnlyCheckInAvailable { get; set; }

        public Guid? CurrentUserId { get; set; }
    }
}

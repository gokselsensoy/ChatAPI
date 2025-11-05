using Application.Abstractions.Messaging;
using Application.Features.Users.DTOs;

namespace Application.Features.Users.Queries.GetMyProfile
{
    public class GetMyProfileQuery : ICachableQuery<UserDto>
    {
        public Guid IdentityId { get; set; }

        // Cache'i IdentityId'ye göre yapıyoruz
        public string CacheKey => $"user:identity:{IdentityId}";
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
    }
}

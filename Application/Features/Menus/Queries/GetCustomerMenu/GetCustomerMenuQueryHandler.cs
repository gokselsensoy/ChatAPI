using Application.Abstractions.QueryRepositories;
using Application.Exceptions;
using Application.Features.Menus.DTOs;
using Domain.Entities;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Menus.Queries.GetCustomerMenu
{
    public class GetCustomerMenuQueryHandler : IRequestHandler<GetCustomerMenuQuery, List<MenuDto>>
    {
        private readonly IMenuQueryRepository _menuQueryRepository;
        private readonly IRepository<UserLocation> _userLocationRepository;

        public GetCustomerMenuQueryHandler(
            IMenuQueryRepository menuQueryRepository,
            IRepository<UserLocation> userLocationRepository)
        {
            _menuQueryRepository = menuQueryRepository;
            _userLocationRepository = userLocationRepository;
        }

        public async Task<List<MenuDto>> Handle(GetCustomerMenuQuery request, CancellationToken cancellationToken)
        {
            // 1. GÜVENLİK (Check-In Kontrolü)
            var userLocation = await _userLocationRepository.GetAsync(
                ul => ul.UserId == request.CurrentUserId,
                cancellationToken);

            if (userLocation == null || userLocation.BranchId != request.BranchId)
            {
                throw new UnauthorizedAccessException("Bu şubenin menüsünü görmek için check-in yapmalısınız.");
            }

            // 2. Menüleri ve Ürünleri Getir
            // Artık çoğul metodu çağırıyoruz
            var menus = await _menuQueryRepository.GetMenusWithItemsByBranchIdAsync(request.BranchId, cancellationToken);

            // 3. Boş Liste Kontrolü (Null yerine .Any() kontrolü daha sağlıklıdır)
            if (menus == null || !menus.Any())
                throw new NotFoundException("Bu şubeye ait aktif bir menü bulunamadı.");

            // 4. Listeyi Döndür
            return menus;
        }
    }
}

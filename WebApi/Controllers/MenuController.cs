using Application.Abstractions.QueryRepositories;
using Application.Features.Menus.Commands.AddMenuItem;
using Application.Features.Menus.Commands.CreateMenu;
using Application.Features.Menus.Commands.DeleteMenu;
using Application.Features.Menus.Commands.DeleteMenuItem;
using Application.Features.Menus.Commands.UpdateMenu;
using Application.Features.Menus.Commands.UpdateMenuItem;
using Application.Features.Menus.DTOs;
using Application.Features.Menus.Queries.GetCustomerMenu;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/menu")]
    public class MenuController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IUserQueryRepository _userQueryRepository;

        public MenuController(ISender sender, IUserQueryRepository userQueryRepository)
        {
            _sender = sender;
            _userQueryRepository = userQueryRepository;
        }

        /// <summary>
        /// Belirtilen şubenin menülerini ve ürünlerini getirir (Sadece Check-In yapmış müşteriler için)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<MenuDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomerMenus([FromQuery] GetCustomerMenuQuery query, CancellationToken cancellationToken)
        {
            query.UserId = await GetActingDomainUserIdAsync(cancellationToken);
            var menus = await _sender.Send(query, cancellationToken);

            // 4. Sonucu 200 OK ile dön
            return Ok(menus);
        }

        /// <summary>
        /// Menü tiplerini istemciye döner.
        /// </summary>
        [HttpGet("menu-types")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetMenuTypes()
        {
            var items = Enum.GetValues<MenuType>()
                .Select(x => new
                {
                    Value = (int)x,
                    Name = x.ToString()
                })
                .ToList();

            return Ok(items);
        }

        /// <summary>
        /// Menü ürün kategori tiplerini istemciye döner.
        /// </summary>
        [HttpGet("category-types")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetCategoryTypes()
        {
            var items = Enum.GetValues<CategoryType>()
                .Select(x => new
                {
                    Value = (int)x,
                    Name = x.ToString()
                })
                .ToList();

            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMenu([FromBody] CreateMenuCommand command, CancellationToken cancellationToken)
        {
            command.ActingUserId = await GetActingDomainUserIdAsync(cancellationToken);
            var menuId = await _sender.Send(command, cancellationToken);
            return Ok(new { Message = "Menü başarıyla oluşturuldu.", MenuId = menuId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenu(Guid id, [FromBody] UpdateMenuCommand command)
        {
            // URL'den gelen ID'yi Command'e atıyoruz ki güvenlik açığı olmasın
            command.MenuId = id;
            await _sender.Send(command);
            return NoContent(); // 204: İşlem başarılı ama dönülecek data yok
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenu(Guid id)
        {
            var command = new DeleteMenuCommand { MenuId = id };
            await _sender.Send(command);
            return NoContent();
        }


        [HttpPost("{menuId}/items")]
        public async Task<IActionResult> AddMenuItem(Guid menuId, [FromBody] AddMenuItemCommand command, CancellationToken cancellationToken)
        {
            command.MenuId = menuId;
            command.ActingUserId = await GetActingDomainUserIdAsync(cancellationToken);
            var itemId = await _sender.Send(command, cancellationToken);

            return Ok(new { Message = "Ürün menüye eklendi.", ItemId = itemId });
        }

        [HttpPut("{menuId}/items/{itemId}")]
        public async Task<IActionResult> UpdateItem(Guid menuId, Guid itemId, [FromBody] UpdateMenuItemCommand command, CancellationToken cancellationToken)
        {
            command.MenuId = menuId;
            command.MenuItemId = itemId;
            command.ActingUserId = await GetActingDomainUserIdAsync(cancellationToken);
            await _sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{menuId}/items/{itemId}")]
        public async Task<IActionResult> DeleteItem(Guid menuId, Guid itemId, CancellationToken cancellationToken)
        {
            var command = new DeleteMenuItemCommand
            {
                MenuId = menuId,
                MenuItemId = itemId,
                ActingUserId = await GetActingDomainUserIdAsync(cancellationToken)
            };
            await _sender.Send(command, cancellationToken);
            return NoContent();
        }

        private async Task<Guid> GetActingDomainUserIdAsync(CancellationToken cancellationToken)
        {
            var identityIdClaim = User.FindFirstValue(OpenIddictConstants.Claims.Subject)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(identityIdClaim) || !Guid.TryParse(identityIdClaim, out var identityId))
                throw new UnauthorizedAccessException("Geçersiz token. Kullanıcı ID bulunamadı.");

            var user = await _userQueryRepository.GetByIdentityIdAsync(identityId, cancellationToken)
                ?? throw new UnauthorizedAccessException("Kullanıcı profili bulunamadı.");

            return user.Id;
        }
    }
}

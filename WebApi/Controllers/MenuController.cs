using Application.Features.Menus.Commands.AddMenuItem;
using Application.Features.Menus.Commands.CreateMenu;
using Application.Features.Menus.Commands.DeleteMenu;
using Application.Features.Menus.Commands.DeleteMenuItem;
using Application.Features.Menus.Commands.UpdateMenu;
using Application.Features.Menus.Commands.UpdateMenuItem;
using Application.Features.Menus.DTOs;
using Application.Features.Menus.Queries.GetCustomerMenu;
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

        public MenuController(ISender sender)
        {
            _sender = sender;
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
            var menus = await _sender.Send(query, cancellationToken);

            // 4. Sonucu 200 OK ile dön
            return Ok(menus);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMenu([FromBody] CreateMenuCommand command)
        {
            var menuId = await _sender.Send(command);
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
        public async Task<IActionResult> AddMenuItem(Guid menuId, [FromBody] AddMenuItemCommand command)
        {
            // Hangi menüye ekleneceğini URL'den alıyoruz
            command.MenuId = menuId;
            var itemId = await _sender.Send(command);

            return Ok(new { Message = "Ürün menüye eklendi.", ItemId = itemId });
        }

        [HttpPut("{menuId}/items/{itemId}")]
        public async Task<IActionResult> UpdateItem(Guid menuId, Guid itemId, [FromBody] UpdateMenuItemCommand command)
        {
            command.MenuId = menuId;
            command.MenuItemId = itemId;
            await _sender.Send(command);
            return NoContent();
        }

        [HttpDelete("{menuId}/items/{itemId}")]
        public async Task<IActionResult> DeleteItem(Guid menuId, Guid itemId)
        {
            var command = new DeleteMenuItemCommand { MenuId = menuId, MenuItemId = itemId };
            await _sender.Send(command);
            return NoContent();
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(OpenIddictConstants.Claims.Subject)
                           ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Geçersiz token. Kullanıcı ID bulunamadı.");
            }

            return userId;
        }
    }
}

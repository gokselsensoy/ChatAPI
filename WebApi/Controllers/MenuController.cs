using Application.Features.Menus.Commands.AddMenuItem;
using Application.Features.Menus.Commands.CreateMenu;
using Application.Features.Menus.Commands.DeleteMenu;
using Application.Features.Menus.Commands.DeleteMenuItem;
using Application.Features.Menus.Commands.UpdateMenu;
using Application.Features.Menus.Commands.UpdateMenuItem;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/menu")]
    public class MenuController : ControllerBase
    {
        private readonly ISender _sender;

        public MenuController(ISender sender)
        {
            _sender = sender;
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
    }
}

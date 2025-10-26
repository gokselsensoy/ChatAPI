using Application.Features.Orders.Commands.CreateOrder;
using Application.Features.Orders.Queries.GetOrderById;
using Application.Features.Orders.Queries.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ISender _sender;

        public OrdersController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Yeni bir sipariş oluşturur.
        /// </summary>
        /// <param name="command">Sipariş oluşturma bilgileri.</param>
        /// <returns>Oluşturulan siparişin ID'si ile birlikte 201 Created yanıtı.</returns>
        /// <response code="201">Sipariş başarıyla oluşturuldu.</response>
        /// <response code="400">Geçersiz istek verisi (Validation hataları).</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
        {
            // İsteği MediatR'e gönderiyoruz.
            // MediatR, bu 'command' için doğru 'handler'ı (CreateOrderCommandHandler)
            // bulup çalıştıracak.
            // ValidationPipelineBehaviour da otomatik olarak araya girecek.
            var orderId = await _sender.Send(command);

            // 201 Created yanıtı ile yeni oluşturulan kaynağın ID'sini dönüyoruz
            return CreatedAtAction(nameof(GetById), new { id = orderId }, command);
        }

        /// <summary>
        /// Belirtilen ID'ye sahip siparişi getirir.
        /// </summary>
        /// <param name="id">Getirilecek siparişin ID'si.</param>
        /// <returns>Sipariş detayları.</returns>
        /// <response code="200">Sipariş bulundu ve döndürüldü.</response>
        /// <response code="404">Belirtilen ID ile sipariş bulunamadı.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetOrderByIdQuery { OrderId = id };
            var orderDto = await _sender.Send(query);

            // (NotFoundException'ı global middleware'de yakalamazsak burada manuel kontrol gerekir)
            // if (orderDto == null) return NotFound(); 

            return Ok(orderDto);
        }
    }
}

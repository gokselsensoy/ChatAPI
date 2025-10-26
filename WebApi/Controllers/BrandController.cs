using Application.Features.Brands.Commands.CreateBrand;
using Application.Features.Brands.Commands.DeleteBrand;
using Application.Features.Brands.Commands.UpdateBrand;
using Application.Features.Brands.DTOs;
using Application.Features.Brands.Queries.GetAllBrands;
using Application.Features.Brands.Queries.GetBrandById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandsController : ControllerBase
    {
        private readonly ISender _sender;

        public BrandsController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Yeni bir marka oluşturur.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateBrandCommand command)
        {
            // ValidationPipelineBehaviour otomatik çalışacak
            var brandId = await _sender.Send(command);

            // 201 Created yanıtı ile yeni oluşturulan kaynağın ID'sini dön
            return CreatedAtAction(nameof(GetById), new { id = brandId }, command);
        }

        /// <summary>
        /// Marka sahibini günceller.
        /// </summary>
        [HttpPut("{id:guid}/owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOwner(Guid id, [FromBody] UpdateBrandOwnerCommand command)
        {
            if (id != command.BrandId)
                return BadRequest("ID uyuşmazlığı.");

            await _sender.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Bir markayı siler.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _sender.Send(new DeleteBrandCommand { BrandId = id });
            return NoContent();
        }

        /// <summary>
        /// Belirtilen ID'ye sahip markayı getirir.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BrandDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetBrandByIdQuery { BrandId = id };
            var brandDto = await _sender.Send(query);

            // NotFoundException, GlobalExceptionHandlingMiddleware
            // tarafından yakalanıp 404'e çevrilir.
            return Ok(brandDto);
        }

        /// <summary>
        /// Tüm markaları listeler.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<BrandDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var brands = await _sender.Send(new GetAllBrandsQuery());
            return Ok(brands);
        }
    }
}

using Application.Features.Branchs.Commands.CreateBranch;
using Application.Features.Branchs.DTOs;
using Application.Features.Branchs.Queries.GetBranchById;
using Application.Features.Branchs.Queries.GetBranchesByBrandId;
using Application.Shared.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/branches")]
    public class BranchController : ControllerBase
    {
        private readonly ISender _sender;

        public BranchController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Belirtilen BrandId altına yeni bir şube oluşturur.
        /// </summary>
        /// <remarks>
        /// Rota: POST /api/brands/{brandId}/branches
        /// </remarks>
        [HttpPost("brands/{brandId:guid}/branch")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(Guid brandId, [FromBody] CreateBranchCommand command)
        {
            // URL'den gelen brandId'yi Command objesine set ediyoruz.
            // ValidationPipeline geri kalanını (Lat/Long zorunlu mu vb.) kontrol eder.
            command.BrandId = brandId;

            var branchId = await _sender.Send(command);

            // 201 Created yanıtı ile yeni şubenin 'GetById' endpoint'ine yönlendiriyoruz
            //
            return CreatedAtAction(nameof(GetById), new { id = branchId }, command);
        }

        /// <summary>
        /// Belirtilen ID'ye sahip şubeyi getirir.
        /// </summary>
        /// <remarks>
        /// Rota: GET /api/branches/{id}
        /// </remarks>
        [HttpGet("branch/{id:guid}")]
        [ProducesResponseType(typeof(BranchDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetBranchByIdQuery { BranchId = id };
            var branchDto = await _sender.Send(query);

            // NotFoundException, GlobalExceptionHandlingMiddleware tarafından yakalanır
            return Ok(branchDto);
        }

        /// <summary>
        /// Belirtilen BrandId'ye ait tüm şubeleri sayfalı olarak listeler.
        /// </summary>
        /// <remarks>
        /// Rota: GET /api/brands/{brandId}/branches?PageNumber=1&PageSize=10
        /// </remarks>
        [HttpGet("brands/{brandId:guid}/branch")]
        [ProducesResponseType(typeof(PaginatedResponse<BranchDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByBrandId(Guid brandId, [FromQuery] PaginatedRequest pagination)
        {
            var query = new GetBranchesByBrandIdQuery
            {
                BrandId = brandId,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            var result = await _sender.Send(query);
            return Ok(result);
        }
    }
}

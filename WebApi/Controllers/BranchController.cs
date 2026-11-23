using Application.Abstractions.QueryRepositories;
using Application.Features.Branchs.Commands.AssignBranchAdmin;
using Application.Features.Branchs.Commands.CreateBranch;
using Application.Features.Branchs.Commands.RemoveBranchAdmin;
using Application.Features.Branchs.Commands.UpdateBranch;
using Application.Features.Branchs.DTOs;
using Application.Features.Branchs.Queries.GetBranchAdmins;
using Application.Features.Branchs.Queries.GetBranchById;
using Application.Features.Branchs.Queries.GetBranchesByBrandId;
using Application.Features.Branchs.Queries.GetNearbyBranches;
using Application.Shared.Pagination;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;
using WebApi.Contracts;

namespace WebApi.Controllers
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/branches")]
    public class BranchController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IUserQueryRepository _userQueryRepository;

        public BranchController(ISender sender, IUserQueryRepository userQueryRepository)
        {
            _sender = sender;
            _userQueryRepository = userQueryRepository;
        }

        /// <summary>
        /// Belirtilen BrandId altına yeni bir şube oluşturur.
        /// </summary>
        /// <remarks>
        /// Rota: POST /api/brands/{brandId}/branches
        /// </remarks>
        [HttpPost("brands/{brandId:guid}")]
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
        /// Belirtilen BranchId'ye ait olan şubeyi günceller.
        /// </summary>
        /// <remarks>
        /// Rota: POST /api/branches/{branchId}
        /// </remarks>
        [HttpPut("branches/{branchId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid branchId, [FromBody] UpdateBranchCommand command)
        {
            // URL'deki ID ile Body'deki ID uyuşmalı
            if (command.BranchId != Guid.Empty && command.BranchId != branchId)
            {
                return BadRequest("URL'deki ID ile gönderilen veri uyuşmuyor.");
            }

            command.BranchId = branchId;

            // Handler çalışsın
            await _sender.Send(command);

            return Ok(new { Message = "Şube başarıyla güncellendi.", Id = branchId });
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
        [HttpGet("brands/{brandId:guid}")]
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

        /// <summary>
        /// Şube etiketlerini (BranchTag) sıra, çeviri anahtarı, ikon ve varsayılan dil etiketleriyle birlikte istemciye döner.
        /// </summary>
        /// <remarks>
        /// Rota: GET /api/branches/tags
        /// </remarks>
        [HttpGet("tags")]
        [ProducesResponseType(typeof(List<BranchTagMetadataDto>), StatusCodes.Status200OK)]
        public IActionResult GetBranchTags()
        {
            var items = new List<BranchTagMetadataDto>
            {
                new((int)BranchTag.Kahve, nameof(BranchTag.Kahve), "tags.coffee", "cafe", 1,
                    new Dictionary<string, string> { ["tr"] = "Kahve", ["en"] = "Coffee" }),
                new((int)BranchTag.CanliMuzik, nameof(BranchTag.CanliMuzik), "tags.liveMusic", "musical-notes", 2,
                    new Dictionary<string, string> { ["tr"] = "Canlı Müzik", ["en"] = "Live Music" }),
                new((int)BranchTag.AcikAlan, nameof(BranchTag.AcikAlan), "tags.openArea", "partly-sunny", 3,
                    new Dictionary<string, string> { ["tr"] = "Açık Alan", ["en"] = "Open Area" }),
                new((int)BranchTag.Alkol, nameof(BranchTag.Alkol), "tags.alcohol", "wine", 4,
                    new Dictionary<string, string> { ["tr"] = "Alkol", ["en"] = "Alcohol" }),
                new((int)BranchTag.Otopark, nameof(BranchTag.Otopark), "tags.parking", "car", 5,
                    new Dictionary<string, string> { ["tr"] = "Otopark", ["en"] = "Parking" }),
                new((int)BranchTag.CalismaAlani, nameof(BranchTag.CalismaAlani), "tags.workspace", "laptop", 6,
                    new Dictionary<string, string> { ["tr"] = "Çalışma Alanı", ["en"] = "Workspace" }),
                new((int)BranchTag.PetFriendly, nameof(BranchTag.PetFriendly), "tags.petFriendly", "paw", 7,
                    new Dictionary<string, string> { ["tr"] = "Evcil Hayvan Dostu", ["en"] = "Pet Friendly" }),
                new((int)BranchTag.VeganSecenekler, nameof(BranchTag.VeganSecenekler), "tags.veganOptions", "leaf", 8,
                    new Dictionary<string, string> { ["tr"] = "Vegan Seçenekler", ["en"] = "Vegan Options" }),
                new((int)BranchTag.Nargile, nameof(BranchTag.Nargile), "tags.hookah", "cloud", 9,
                    new Dictionary<string, string> { ["tr"] = "Nargile", ["en"] = "Hookah" }),
                new((int)BranchTag.DenizKenari, nameof(BranchTag.DenizKenari), "tags.seaside", "water", 10,
                    new Dictionary<string, string> { ["tr"] = "Deniz Kenarı", ["en"] = "Seaside" }),
                new((int)BranchTag.CanliSporYayini, nameof(BranchTag.CanliSporYayini), "tags.liveSports", "football", 11,
                    new Dictionary<string, string> { ["tr"] = "Canlı Spor Yayını", ["en"] = "Live Sports" }),
                new((int)BranchTag.OyunAlani, nameof(BranchTag.OyunAlani), "tags.gameArea", "game-controller", 12,
                    new Dictionary<string, string> { ["tr"] = "Oyun Alanı", ["en"] = "Game Area" }),
                new((int)BranchTag.SigaraIcebilirAlan, nameof(BranchTag.SigaraIcebilirAlan), "tags.smokingArea", "flame", 13,
                    new Dictionary<string, string> { ["tr"] = "Sigara İçilebilir Alan", ["en"] = "Smoking Area" }),
            };

            // Enum'a yeni bir değer eklenip yukarıdaki listeye eklenmesi unutulursa otomatik fallback olarak ekle
            var definedIds = items.Select(x => x.Id).ToHashSet();
            var nextOrder = items.Count + 1;
            foreach (var tag in Enum.GetValues<BranchTag>())
            {
                var id = (int)tag;
                if (!definedIds.Contains(id))
                {
                    var name = tag.ToString();
                    items.Add(new BranchTagMetadataDto(
                        id,
                        name,
                        $"tags.{char.ToLowerInvariant(name[0])}{name[1..]}",
                        "pricetag",
                        nextOrder++,
                        new Dictionary<string, string> { ["tr"] = name, ["en"] = name }
                    ));
                }
            }

            return Ok(items.OrderBy(x => x.Order).ToList());
        }

        /// <summary>
        /// Belirtilen konuma yakın olan şubeleri mesafeye göre sıralı listeler.
        /// Birden fazla etiket gönderildiğinde seçilen tüm etiketleri içeren (VE / AND) şubeler döner.
        /// Tags gönderilmezse veya boş/null ise etiket filtresi uygulanmaz.
        /// </summary>
        /// <remarks>
        /// Rota: GET /api/branches/nearby?Latitude=40.71&amp;Longitude=-74.00&amp;Tags=Kahve&amp;Tags=CanliMuzik
        /// </remarks>
        [HttpGet("nearby")]
        [ProducesResponseType(typeof(NearbyBranchesResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNearby([FromQuery] GetNearbyBranchesQuery query, CancellationToken cancellationToken)
        {
            query.CurrentUserId = await GetActingDomainUserIdAsync(cancellationToken);
            // CachingPipelineBehaviour devreye girecek
            var branches = await _sender.Send(query, cancellationToken);
            return Ok(branches);
        }


        /// <summary>
        /// Şube yöneticilerini listeler (marka sahibi + BranchAdminMap ile atanmış kullanıcılar).
        /// </summary>
        [HttpGet("{branchId:guid}/admins")]
        [ProducesResponseType(typeof(List<BranchAdminListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBranchAdmins(Guid branchId, CancellationToken cancellationToken)
        {
            var query = new GetBranchAdminsQuery
            {
                BranchId = branchId,
                ActingUserId = await GetActingDomainUserIdAsync(cancellationToken)
            };
            var items = await _sender.Send(query, cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Kullanıcıyı bu şubede atanmış yönetici yapar (BranchAdminMap). Marka sahibi atanamaz.
        /// </summary>
        [HttpPost("{branchId:guid}/admins")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AssignBranchAdmin(Guid branchId, [FromBody] AssignBranchAdminRequest body, CancellationToken cancellationToken)
        {
            var command = new AssignBranchAdminCommand
            {
                BranchId = branchId,
                UserId = body.UserId,
                ActingUserId = await GetActingDomainUserIdAsync(cancellationToken)
            };
            await _sender.Send(command, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Kullanıcının bu şubedeki atanmış yöneticiliğini kaldırır (yalnızca BranchAdminMap kaydı).
        /// </summary>
        [HttpDelete("{branchId:guid}/admins/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveBranchAdmin(Guid branchId, Guid userId, CancellationToken cancellationToken)
        {
            var command = new RemoveBranchAdminCommand
            {
                BranchId = branchId,
                UserId = userId,
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

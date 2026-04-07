using Application.Abstractions.QueryRepositories;
using Application.Features.Blacklists.Commands.BanUser;
using Application.Features.Blacklists.Commands.LiftBan;
using Application.Features.Blacklists.Commands.UpdateBan;
using Application.Features.Blacklists.Queries.GetBannedUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [Route("api/branches/{branchId}/blacklist")]
    [ApiController]
    public class BranchBlacklistController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IUserQueryRepository _userQueryRepository;

        public BranchBlacklistController(ISender sender, IUserQueryRepository userQueryRepository)
        {
            _sender = sender;
            _userQueryRepository = userQueryRepository;
        }

        /// <summary>
        /// Şubedeki yasaklı (banlı) kullanıcıları listeler.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBannedUsers(Guid branchId)
        {
            var query = new GetBannedUsersQuery { BranchId = branchId };
            var result = await _sender.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Kullanıcıyı şubeden banlar. (Daha önce yazdığımız BanUserCommand)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BanUser(Guid branchId, [FromBody] BanUserCommand command, CancellationToken cancellationToken)
        {
            command.BranchId = branchId;
            command.ActingUserId = await GetActingDomainUserIdAsync(cancellationToken);
            await _sender.Send(command, cancellationToken);

            return Ok(new { Message = "Kullanıcı başarıyla şubeden uzaklaştırıldı." });
        }

        /// <summary>
        /// Kullanıcının ceza (ban) süresini günceller/uzatır.
        /// </summary>
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateBan(Guid branchId, Guid userId, [FromBody] UpdateBanCommand command, CancellationToken cancellationToken)
        {
            command.BranchId = branchId;
            command.UserId = userId;
            command.ActingUserId = await GetActingDomainUserIdAsync(cancellationToken);
            await _sender.Send(command, cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Kullanıcının banını kaldırır (Affeder).
        /// </summary>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> LiftBan(Guid branchId, Guid userId, CancellationToken cancellationToken)
        {
            var command = new LiftBanCommand
            {
                BranchId = branchId,
                UserId = userId,
                ActingUserId = await GetActingDomainUserIdAsync(cancellationToken)
            };

            await _sender.Send(command, cancellationToken);

            return Ok(new { Message = "Kullanıcının yasağı kaldırıldı." });
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

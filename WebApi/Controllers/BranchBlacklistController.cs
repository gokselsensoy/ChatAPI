using Application.Features.Blacklists.Commands.BanUser;
using Application.Features.Blacklists.Commands.LiftBan;
using Application.Features.Blacklists.Commands.UpdateBan;
using Application.Features.Blacklists.Queries.GetBannedUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    // URL Yapısı: api/branches/{branchId}/blacklist
    [Route("api/branches/{branchId}/blacklist")]
    [ApiController]
    [Authorize] // İdeal olanı [Authorize(Roles = "Admin")] yapmaktır.
    public class BranchBlacklistController : ControllerBase
    {
        private readonly ISender _sender;

        public BranchBlacklistController(ISender sender)
        {
            _sender = sender;
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
        public async Task<IActionResult> BanUser(Guid branchId, [FromBody] BanUserCommand command)
        {
            // URL'den gelen branchId'yi Command'e atıyoruz ki güvenlik açığı olmasın
            command.BranchId = branchId;
            await _sender.Send(command);

            return Ok(new { Message = "Kullanıcı başarıyla şubeden uzaklaştırıldı." });
        }

        /// <summary>
        /// Kullanıcının ceza (ban) süresini günceller/uzatır.
        /// </summary>
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateBan(Guid branchId, Guid userId, [FromBody] UpdateBanCommand command)
        {
            command.BranchId = branchId;
            command.UserId = userId;
            await _sender.Send(command);

            return NoContent();
        }

        /// <summary>
        /// Kullanıcının banını kaldırır (Affeder).
        /// </summary>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> LiftBan(Guid branchId, Guid userId)
        {
            var command = new LiftBanCommand
            {
                BranchId = branchId,
                UserId = userId
            };

            await _sender.Send(command);

            return Ok(new { Message = "Kullanıcının yasağı kaldırıldı." });
        }
    }
}

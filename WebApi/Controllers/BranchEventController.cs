using Application.Features.BranchEvents.Commands.ToggleParticipation;
using Application.Features.BranchEvents.Commands.ToggleReminder;
using Application.Features.BranchEvents.Queries.GetBranchEvents;
using Application.Features.Users.Queries.GetUserByIdentityId; // ChatHub refactor'ünde yazdığımız Query
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/branchevents")]
    public class BranchEventController : ControllerBase
    {
        private readonly ISender _sender;

        public BranchEventController(ISender sender)
        {
            _sender = sender;
        }

        // --- YARDIMCI METOT ---
        // Token'dan IdentityId'yi bulup, bizim sistemimizdeki asıl Guid UserId'ye çevirir
        private async Task<Guid> GetCurrentUserIdAsync()
        {
            var identityIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            
            if (string.IsNullOrWhiteSpace(identityIdClaim) || !Guid.TryParse(identityIdClaim, out var identityId))
                throw new UnauthorizedAccessException("Geçersiz veya eksik Token.");

            // Önceden yazdığımız Query'yi kullanarak kullanıcıyı çekiyoruz (Repository kirliliği yok!)
            var user = await _sender.Send(new GetUserByIdentityIdQuery { IdentityId = identityId });
            
            if (user == null) 
                throw new UnauthorizedAccessException("Kullanıcı doğrulanamadı.");
            
            return user.Id;
        }

        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetEventsByBranch(Guid branchId)
        {
            var userId = await GetCurrentUserIdAsync();

            var query = new GetBranchEventsQuery
            {
                BranchId = branchId,
                UserId = userId
            };

            var result = await _sender.Send(query);
            return Ok(result);
        }

        [HttpPost("participate/{id}")]
        public async Task<IActionResult> ToggleParticipation(Guid id)
        {
            var userId = await GetCurrentUserIdAsync();

            var command = new ToggleEventParticipationCommand
            {
                EventId = id,
                UserId = userId
            };

            // Dönen sonuç (true/false) mobildeki butonu güncellemek için kullanılacak
            var isAttending = await _sender.Send(command);
            
            return Ok(new { success = true, isAttending });
        }

        [HttpPost("reminder/{id}")]
        public async Task<IActionResult> ToggleReminder(Guid id)
        {
            var userId = await GetCurrentUserIdAsync();

            var command = new ToggleEventReminderCommand
            {
                EventId = id,
                UserId = userId
            };

            // Dönen sonuç mobildeki "Hatırlatıcı Kuruldu" ibaresini göstermek için kullanılacak
            var isReminderSet = await _sender.Send(command);
            
            return Ok(new { success = true, isReminderSet });
        }
    }
}
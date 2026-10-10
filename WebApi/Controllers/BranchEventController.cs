using Application.Features.BranchEvents.Commands.CreateBranchEvent;
using Application.Features.BranchEvents.Commands.DeleteBranchEvent;
using Application.Features.BranchEvents.Commands.ToggleParticipation;
using Application.Features.BranchEvents.Commands.ToggleReminder;
using Application.Features.BranchEvents.Commands.UpdateBranchEvent;
using Application.Features.BranchEvents.Queries.GetBranchEvents;
using Application.Features.Users.Queries.GetUserByIdentityId;
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

        private async Task<Guid> GetCurrentUserIdAsync()
        {
            var identityIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(identityIdClaim) || !Guid.TryParse(identityIdClaim, out var identityId))
                throw new UnauthorizedAccessException("Geçersiz veya eksik Token.");

            var user = await _sender.Send(new GetUserByIdentityIdQuery { IdentityId = identityId });

            if (user == null)
                throw new UnauthorizedAccessException("Kullanıcı doğrulanamadı.");

            return user.Id;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateEvent([FromBody] CreateBranchEventCommand command)
        {
            var eventId = await _sender.Send(command);
            return Ok(new { success = true, eventId });
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateBranchEventCommand command)
        {
            command.EventId = id;
            await _sender.Send(command);
            return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            await _sender.Send(new DeleteBranchEventCommand { EventId = id });
            return Ok(new { success = true });
        }

        [HttpGet("branch/{branchId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetEventsByBranch(Guid branchId)
        {
            var userId = await GetCurrentUserIdAsync();
            var query = new GetBranchEventsQuery { BranchId = branchId, UserId = userId };
            var result = await _sender.Send(query);

            return Ok(result);
        }

        [HttpPost("participate/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleParticipation(Guid id)
        {
            var userId = await GetCurrentUserIdAsync();
            var command = new ToggleEventParticipationCommand { EventId = id, UserId = userId };
            var isAttending = await _sender.Send(command);

            return Ok(new { success = true, isAttending });
        }

        [HttpPost("reminder/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleReminder(Guid id)
        {
            var userId = await GetCurrentUserIdAsync();
            var command = new ToggleEventReminderCommand { EventId = id, UserId = userId };
            var isReminderSet = await _sender.Send(command);

            return Ok(new { success = true, isReminderSet });
        }
    }
}
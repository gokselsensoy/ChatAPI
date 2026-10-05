using Application.Abstractions.Messaging;
using System;
using System.Text.Json.Serialization;

namespace Application.Features.BranchEvents.Commands.ToggleReminder
{
    public class ToggleEventReminderCommand : ICommand<bool>
    {
        public Guid EventId { get; set; }
        
        [JsonIgnore]
        public Guid UserId { get; set; }
    }
}
using Application.Abstractions.Messaging;
using System;
using System.Text.Json.Serialization;

namespace Application.Features.BranchEvents.Commands.ToggleParticipation
{
    public class ToggleEventParticipationCommand : ICommand<bool>
    {
        public Guid EventId { get; set; }

        [JsonIgnore] // Güvenlik: Controller'da Token'dan doldurulacak
        public Guid UserId { get; set; }
    }
}
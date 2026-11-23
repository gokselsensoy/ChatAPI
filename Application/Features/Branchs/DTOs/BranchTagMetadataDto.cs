using System.Collections.Generic;

namespace Application.Features.Branchs.DTOs
{
    public record BranchTagMetadataDto(
        int Id,
        string Key,
        string TranslationKey,
        string Icon,
        int Order,
        IReadOnlyDictionary<string, string> DefaultLabels
    );
}

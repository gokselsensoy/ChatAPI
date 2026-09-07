using MediatR;
using System.Collections.Generic;

namespace Application.Features.Branchs.Queries.GetPredefinedTags
{
    public class GetPredefinedTagsQuery : IRequest<List<string>>
    {
    }
}

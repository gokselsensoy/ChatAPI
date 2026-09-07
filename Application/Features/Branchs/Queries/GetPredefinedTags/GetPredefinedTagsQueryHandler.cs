using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Constants;

namespace Application.Features.Branchs.Queries.GetPredefinedTags
{
    public class GetPredefinedTagsQueryHandler : IRequestHandler<GetPredefinedTagsQuery, List<string>>
    {
        public Task<List<string>> Handle(GetPredefinedTagsQuery request, CancellationToken cancellationToken)
        {
            // Şimdilik sabit listeden dönüyoruz.
            return Task.FromResult(PredefinedTags.Tags.ToList());
        }
    }
}

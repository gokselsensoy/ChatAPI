using MediatR;
using System;

namespace Application.Features.Branches.Queries.CheckUserBranchAccess
{
    // İşlem sonucunda şubeye yetkisi var mı yok mu (bool) bilgisini dönecek.
    public class CheckUserBranchAccessQuery : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public Guid BranchId { get; set; }
    }
}

namespace Application.Features.Branches.Queries.CheckUserBranchAccess
{
}
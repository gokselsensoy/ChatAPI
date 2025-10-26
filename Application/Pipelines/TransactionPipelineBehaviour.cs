using Application.Abstractions.Messaging;
using Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Pipelines
{
    public class TransactionPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<TransactionPipelineBehaviour<TRequest, TResponse>> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public TransactionPipelineBehaviour(
            ILogger<TransactionPipelineBehaviour<TRequest, TResponse>> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var isCommand = typeof(TRequest).GetInterfaces().Any(i =>
                i == typeof(ICommand) || (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>)));

            if (!isCommand)
            {
                return await next();
            }

            if (_unitOfWork.HasActiveTransaction)
            {
                return await next();
            }

            var requestName = typeof(TRequest).Name;

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                _logger.LogInformation("Transaction Başlatıldı: {RequestName}", requestName);

                var response = await next();

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                _logger.LogInformation("Transaction Onaylandı (Commit): {RequestName}", requestName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction Sırasında Hata: {RequestName}. Transaction Geri Alınıyor (Rollback)...", requestName);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                throw;
            }
        }
    }
}

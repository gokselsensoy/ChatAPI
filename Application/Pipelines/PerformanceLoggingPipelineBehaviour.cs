using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Pipelines
{
    public class PerformanceLoggingPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<PerformanceLoggingPipelineBehaviour<TRequest, TResponse>> _logger;
        private readonly Stopwatch _stopwatch;

        public PerformanceLoggingPipelineBehaviour(ILogger<PerformanceLoggingPipelineBehaviour<TRequest, TResponse>> logger)
        {
            _logger = logger;
            _stopwatch = new Stopwatch();
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogDebug("İşlem Başlıyor: {RequestName} ({@Request})", requestName, request);
            _stopwatch.Start();

            try
            {
                var response = await next();

                _stopwatch.Stop();
                _logger.LogDebug("İşlem Tamamlandı: {RequestName}. Süre: {ElapsedMilliseconds} ms.",
                    requestName,
                    _stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                _stopwatch.Stop();
                _logger.LogError(ex, "İşlem Sırasında Hata: {RequestName}. Süre: {ElapsedMilliseconds} ms. Hata: {ErrorMessage}",
                    requestName,
                    _stopwatch.ElapsedMilliseconds,
                    ex.Message);

                throw;
            }
        }
    }
}

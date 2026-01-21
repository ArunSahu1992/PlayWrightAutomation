using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;


namespace Configuration
{
    public static class RetryHelper
    {
        public static async Task<T> ExecuteAsync<T>(
            Func<Task<T>> action,
            int maxRetries = 3,
            int delayMilliseconds = 4000,
            ILogger? logger = null,
            Func<Exception, bool>? shouldRetry = null)
        {
            shouldRetry ??= _ => true;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (attempt < maxRetries && shouldRetry(ex))
                {
                    logger?.LogInformation(
                        $"Retry {attempt}/{maxRetries} failed: {ex.Message}"
                    );
                    await Task.Delay(delayMilliseconds);
                }
            }
            throw new Exception("Operation failed after maximum retries");
        }

        public static async Task ExecuteWithRetryAsync(
               Func<Task> testExecution,
               int retryCount, CancellationToken token, ILogger? _logger = null)
        {
            Exception lastException = null;

            for (int attempt = 1; attempt <= retryCount; attempt++)
            {
                try
                {
                    await testExecution();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Retrying the above testcase for attempt {attempt}.");
                    lastException = ex;
                    if (attempt == retryCount)
                        break;
                }
            }

            throw lastException;
        }
    }

}

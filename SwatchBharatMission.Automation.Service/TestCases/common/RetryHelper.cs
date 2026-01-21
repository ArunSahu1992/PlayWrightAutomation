using Automation.Core;
using Microsoft.Playwright;
using Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using TestCases.common;
using System.Net;


namespace TestCases.common
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
    }
    public record ApiResult(string Content, HttpStatusCode StatusCode);
}

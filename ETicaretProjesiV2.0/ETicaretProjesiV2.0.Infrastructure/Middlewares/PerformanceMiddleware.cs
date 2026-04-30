using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ETicaretProjesiV2._0.Infrastructure.Middlewares
{
    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMiddleware> _logger;

        public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            await _next(context);

            sw.Stop();
            var elapsedMiliseconds = sw.ElapsedMilliseconds;

            if (elapsedMiliseconds > 500)
            {
                var requestPath = context.Request.Path;
                var method = context.Request.Method;
                _logger.LogWarning($"Performans Uyarısı: {method} {requestPath} harcanan süre {elapsedMiliseconds}ms");
            }
            else
            {
                _logger.LogInformation($"⏱️ İşlem Süresi: {elapsedMiliseconds}ms");
            }
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ETicaretProjesiV2._0.Infrastructure.Middlewares
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestBody = await GetRequestBody(context.Request);
            var maskedRequest = MaskSensitiveData(requestBody);
            _logger.LogInformation($"--- [API REQ] {context.Request.Method} {context.Request.Path}---\nBody:{maskedRequest}");

            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            try
            {
                await _next(context);

                var responseText = await GetResponseBody(context.Response);
                _logger.LogInformation($"--- [API RES] {context.Response.StatusCode} ---\nBody:{responseText}");
            }
            finally
            {

                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
            }

        }

        private async Task<string> GetRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }

        private async Task<string> GetResponseBody(HttpResponse response)
        {
            response.Body.Seek(0,SeekOrigin.Begin);
            string body = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0,SeekOrigin.Begin);

            return body;
        }

        private string MaskSensitiveData(string json)
        {
            if (string.IsNullOrEmpty(json)) return json;
            return Regex.Replace(json, @"(?i)(""password""\s*:\s*"")([^""]+)("")", "$1********$3");
        }

    }
}

using ETicaretProjesiV2._0.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ETicaretProjesiV2._0.Infrastructure.Middlewares
{
    public class GlobalExceptionMiddlerware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddlerware> _logger;
        public GlobalExceptionMiddlerware(RequestDelegate next, ILogger<GlobalExceptionMiddlerware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {

                _logger.LogError($"Aga bir hata yakalandı: {ex.Message}");
                await HandleExceptionAsync(context, ex);
            }
        }
        private static Task HandleExceptionAsync(HttpContext context ,Exception exception)
        {
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorResponse = new ErrorDetails
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message
            };
            return context.Response.WriteAsync(errorResponse.ToString());
        }
    }
}

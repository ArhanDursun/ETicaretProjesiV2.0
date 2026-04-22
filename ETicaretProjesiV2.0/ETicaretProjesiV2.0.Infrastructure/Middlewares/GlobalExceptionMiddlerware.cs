using ETicaretProjesiV2._0.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;

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

                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("Response Zaten Başladığı için hata mesajı gönderilmedi");
                    return;
                }
                await HandleExceptionAsync(context, ex);
            }
        }
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

          
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var message = exception.Message;

           
            if (exception.Message == "Giriş Bilgileri Hatalı" ||
                exception.Message == "Kullanıcı bulunamadı" ||
                exception.Message == "Lütfen önce emailinize gelen doğrulama kodunu giriniz")
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }

            var response = new { message = message };
            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}

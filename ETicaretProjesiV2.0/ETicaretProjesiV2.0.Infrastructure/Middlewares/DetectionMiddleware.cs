using ETicaretProjesiV2._0.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ETicaretProjesiV2._0.Infrastructure.Middlewares
{
    public class DetectionMiddleware
    {
        private readonly RequestDelegate _next;
        public DetectionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,IDistributedCache cache)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var currentIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";
                var incomingDeviceData = context.Request.Headers["User-Agent"].ToString();
                var cacheKey = $"UserSession_{userId}";

                var existingSessionJson = await cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(existingSessionJson))
                {
                    var lastSession = JsonSerializer.Deserialize<UserFingerPrint>(existingSessionJson);

                    if (lastSession.IpAddress != currentIp || lastSession.UserDevice != incomingDeviceData)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[FRAUD ALARM] ⚠️ Kullanıcı {userId} şüpheli bir cihaz/IP değişimi yaptı!");
                        Console.WriteLine($"ESKİ: {lastSession.IpAddress} - {lastSession.UserDevice}");
                        Console.WriteLine($"YENİ: {currentIp} - {incomingDeviceData}");
                        Console.ResetColor();

                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync("Supheli hareket tespit edildi. Guvenliginiz icin oturum durduruldu");
                        return;
                    }
                }
                var newFingerPrint = new UserFingerPrint
                {
                    IpAddress = currentIp,
                    UserDevice = incomingDeviceData,
                    LastActivity = DateTime.UtcNow,
                };

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(1)
                };

                await cache.SetStringAsync(cacheKey,JsonSerializer.Serialize(newFingerPrint),cacheOptions);
            }
            await _next(context);
        }
    }
}

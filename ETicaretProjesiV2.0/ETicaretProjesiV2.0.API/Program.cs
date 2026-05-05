using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using ETicaretProjesiV2._0.API.Controllers;
using ETicaretProjesiV2._0.Application.Consumers;
using ETicaretProjesiV2._0.Application.Interfaces;
using ETicaretProjesiV2._0.Application.Interfaces.Repositories;
using ETicaretProjesiV2._0.Application.Interfaces.Services;
using ETicaretProjesiV2._0.Application.Mapping;
using ETicaretProjesiV2._0.Application.Models;
using ETicaretProjesiV2._0.Application.Services;
using ETicaretProjesiV2._0.Entities;
using ETicaretProjesiV2._0.Infrastructure.Hubs;
using ETicaretProjesiV2._0.Infrastructure.Middlewares;
using ETicaretProjesiV2._0.Infrastructure.Services;
using ETicaretProjesiV2._0.Persistence.Context;
using ETicaretProjesiV2._0.Persistence.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) 
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning) 
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
var tokenSettings = builder.Configuration.GetSection("Token").Get<TokenSettings>();
builder.Services.AddSingleton(tokenSettings);
builder.Services.AddSingleton<VisitorStorage>();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();


builder.Services.AddIdentity<AppUser, AppRole>(options => {
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOfferService,OfferService>();
builder.Services.AddScoped<IBasketService, BasketService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminManagementService, AdminManagementService>();
builder.Services.AddScoped<IProductInteractionService, ProductInteractionService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISupportService, SupportServices>();
builder.Services.AddScoped<IDirectMessageService, DirectMessageService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<ISignalService,SignalRService>();
builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
builder.Services.AddScoped<IPaymentService, PaymentManager>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.SecurityKey)),
        ValidateIssuer = true,
        ValidIssuer = tokenSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = tokenSettings.Audience,
        ValidateLifetime = true
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if(!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/chathub")|| path.StartsWithSegments("/supporthub") || path.StartsWithSegments("/notificationhub") || path.StartsWithSegments("/traffichub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };

    
});

builder.Services.AddControllers().AddJsonOptions(x=>x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
builder.Services.AddSignalR(options =>
{

    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
}).AddStackExchangeRedis("localhost:6379", options =>
{
    options.Configuration.ChannelPrefix = "E-Ticaret_SignalR";
});
    
    
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();

builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile(new GeneralMapping());
});
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(15);
});
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379"; 
    options.InstanceName = "E-Ticaret";
});
var redisEndpoints = new List<RedLockEndPoint> { new RedLockEndPoint { EndPoint = new System.Net.DnsEndPoint("localhost", 6379) } };
builder.Services.AddSingleton<IDistributedLockFactory>(RedLockFactory.Create(redisEndpoints));
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false"));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<GenerateReportEventConsumer>();
    x.AddConsumer<ProductPriceChangedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});
var app = builder.Build();
app.UseMiddleware<GlobalExceptionMiddlerware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware <PerformanceMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

       
        await ETicaretProjesiV2._0.Persistence.Configurations.UserAndRoleSeeder.SeedAsync(roleManager, userManager);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Seed sırasında hata oluştu: " + ex.Message);
    }
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("xyz")
            .WithTheme(ScalarTheme.Mars) 
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication(); 
app.UseAuthorization();
app.UseMiddleware<VisitorTrackingMiddleware>();
app.UseMiddleware<DetectionMiddleware>();


app.UseStaticFiles();

app.MapControllers();
app.MapHub<SupportHub>("/supporthub");
app.MapHub<ChatHub>("/chathub");
app.MapHub<TrafficHub>("/traffichub");
app.MapHub<NotificationHub>("/notificationhub");

app.Run();
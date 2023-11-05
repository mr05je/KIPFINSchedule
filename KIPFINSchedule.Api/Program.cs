using System.Text;
using KIPFINSchedule.Api;
using KIPFINSchedule.Api.Controllers;
using KIPFINSchedule.Api.Services;
using KIPFINSchedule.Api.Services.ScheduleServices;
using KIPFINSchedule.Api.Services.UtilServices;
using KIPFINSchedule.Core.Parser;
using KIPFINSchedule.Core.Parser.Bypass;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using OfficeOpenXml;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Telegram.Bot;
using AppContext = KIPFINSchedule.Database.AppContext;
using ILogger = Serilog.ILogger;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(LogEventLevel.Debug)
    .WriteTo.File($"./logs/app-{DateTime.Now:yy-MM-dd hh-mm-ss}.log")
    .CreateLogger();

try
{
    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

    #region builder

    var builder = WebApplication.CreateBuilder(args);

    ILogger appContextLogger = new LoggerConfiguration()
        .WriteTo.File($"./logs/db/db-{DateTime.Now:yy-MM-dd hh-mm-ss}.log").CreateLogger();
    
    var botConfigSection = builder.Configuration.GetSection("BotConfiguration");
    
    builder.Services.AddDbContext<AppContext>((sp, optionsBuilder) =>
        optionsBuilder.UseNpgsql(sp.GetService<IConfiguration>()!.GetConnectionString("localhost"))
            .UseLoggerFactory(new SerilogLoggerFactory(appContextLogger)), ServiceLifetime.Singleton);

    builder.Logging.ClearProviders();
    builder.Services.AddSerilog(Log.Logger);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.Configure<BotConfiguration>(botConfigSection);

    var botConfig = botConfigSection.Get<BotConfiguration>() ?? throw new Exception("Can't get bot config!");

    var bypass = builder.Configuration.GetSection("Bypass").Get<BypassCred>()!;

    builder.Services.AddSingleton(bypass);
    
    builder.Services.AddHttpClient("TELEGRAM_CLIENT")
        .AddTypedClient<ITelegramBotClient>(httpClient =>
        {
            TelegramBotClientOptions options = new(botConfig.BotToken);
            return new TelegramBotClient(options, httpClient);
        });

    builder.Services.AddScoped<UpdateHandlers>();
    builder.Services.AddHostedService<ConfigureWebhook>();
    builder.Services.AddHostedService<SubscriptionService>();
    
    builder.Services.AddSingleton<ParserBridge>();
    builder.Services.AddScoped<ScheduleParser>();

    builder.Services.AddHostedService<AutoScheduleService>();
    builder.Services.AddHostedService<SubscriberAutoScheduleService>();
    
    builder.Services
        .AddControllers()
        .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        });
    
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey
                (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true
        };
    });
    
    builder.Services.AddAuthorization();

    builder.Services.AddSingleton<JwtService>();

    #endregion

    #region app
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapBotWebhookRoute<BotController>(botConfig.Route);
    app.MapControllers();
    
    app.UseCors(x => x
        .AllowAnyMethod()
        .AllowAnyHeader()
        .SetIsOriginAllowed(_ => true) 
        .AllowCredentials());
    
    app.Run();

    #endregion
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
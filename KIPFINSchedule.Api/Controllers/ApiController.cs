using System.Globalization;
using KIPFINSchedule.Api.Filters;
using KIPFINSchedule.Api.Models;
using KIPFINSchedule.Api.Services.ScheduleServices;
using KIPFINSchedule.Api.Services.UtilServices;
using KIPFINSchedule.Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Telegram.Bot;
using AppContext = KIPFINSchedule.Database.AppContext;

// ReSharper disable InconsistentNaming

namespace KIPFINSchedule.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class ApiController : ControllerBase
{
    private const string BaseScheduleFormat = 
        """
        Вот твоё расписание на {{date}}📚:
        {{schedule}}
        """;
    private readonly JwtService _jwtService;
    private readonly BotConfiguration _botConfiguration;
    private readonly ITelegramBotClient _botClient;

    public ApiController(JwtService jwtService, IOptions<BotConfiguration> botConfiguration, ITelegramBotClient botClient)
    {
        _jwtService = jwtService;
        _botClient = botClient;
        _botConfiguration = botConfiguration.Value;
    }
    

    [Authorize]
    [ValidateApiSubscription]
    [HttpGet("gaf")]
    public async Task<IActionResult> GetAF([FromServices] AppContext appContext)
    {
        var accessToken = Request.Headers[HeaderNames.Authorization];
        
        var id = _jwtService.ValidateToken(accessToken.ToString()).Claims.First().Value;

        var settings = await appContext.Settings.FirstOrDefaultAsync(x => x.Id.ToString() == id);

        if (settings != null)
            return Ok(new GetFormat { Format = settings.ASMessageFormat ?? BaseMessageTemplate.BaseMessageText });
        
        settings = new SettingEntity
        {
            ASMessageFormat = BaseMessageTemplate.BaseMessageText, ItemFormat = BaseMessageTemplate.BaseItemFormat,
            GPSMessageFormat = BaseScheduleFormat, Id = Convert.ToInt64(id)
        };
        await appContext.AddAsync(settings);
        await appContext.SaveChangesAsync();

        return Ok(new GetFormat { Format = settings.ASMessageFormat ?? BaseMessageTemplate.BaseMessageText});
    }
    
    [Authorize]
    [ValidateApiSubscription]
    [HttpGet("gf")]
    public async Task<IActionResult> GetF([FromServices] AppContext appContext)
    {
        var accessToken = Request.Headers[HeaderNames.Authorization];
        
        var id = _jwtService.ValidateToken(accessToken.ToString()).Claims.First().Value;

        var settings = await appContext.Settings.FirstOrDefaultAsync(x => x.Id.ToString() == id);

        if (settings != null) return Ok(new GetFormat { Format = settings.GPSMessageFormat ?? BaseScheduleFormat });
        settings = new SettingEntity
        {
            ASMessageFormat = BaseMessageTemplate.BaseMessageText, ItemFormat = BaseMessageTemplate.BaseItemFormat,
            GPSMessageFormat = BaseScheduleFormat, Id = Convert.ToInt64(id)
        };

        await appContext.AddAsync(settings);
        await appContext.SaveChangesAsync();

        return Ok(new GetFormat { Format = settings.GPSMessageFormat ?? BaseScheduleFormat});
    }
    
    [Authorize]
    [ValidateApiSubscription]
    [HttpGet("ge")]
    public async Task<IActionResult> GetEtc([FromServices] AppContext appContext)
    {
        var accessToken = Request.Headers[HeaderNames.Authorization];
        
        var id = _jwtService.ValidateToken(accessToken.ToString()).Claims.First().Value;

        var settings = await appContext.Settings.FirstOrDefaultAsync(x => x.Id.ToString() == id);

        var user = await appContext.Users.FirstOrDefaultAsync(x => x.Id.ToString() == id);

        if (user is not { AutoSchedule: true })
        {
            if (user == null)
            {
                user = new UserEntity
                {
                    Id = long.Parse(id),
                    AutoSchedule = true
                };

                await appContext.AddAsync(user);
                await appContext.SaveChangesAsync();

                return BadRequest();
            }
            
            if (string.IsNullOrEmpty(user.Group))
            {
                return NotFound();
            }
            
            user.AutoSchedule = true;
            
            await appContext.SaveChangesAsync();

            await _botClient.SendTextMessageAsync(long.Parse(id), "Автоматическое было включено!");
        }

        if (settings != null)
            return Ok(new GetEtc
            {
                ItemFormat = settings.ItemFormat ?? BaseMessageTemplate.BaseItemFormat,
                MondayTime = settings.MondayTime.ToString(@"hh\:mm"), UseMondayTime = settings.UseMondayTime,
                OtherDays = new List<string>(new[]
                {
                    settings.TuesdayTime.ToString(@"hh\:mm"),
                    settings.WednesdayTime.ToString(@"hh\:mm"),
                    settings.ThursdayTime.ToString(@"hh\:mm"),
                    settings.FridayTime.ToString(@"hh\:mm")
                })
            });
        settings = new SettingEntity
        {
            ASMessageFormat = BaseMessageTemplate.BaseMessageText, ItemFormat = BaseMessageTemplate.BaseItemFormat,
            GPSMessageFormat = BaseScheduleFormat, Id = Convert.ToInt64(id)
        };

        await appContext.AddAsync(settings);
        await appContext.SaveChangesAsync();

        return Ok(new GetEtc { ItemFormat = settings.ItemFormat ?? BaseMessageTemplate.BaseItemFormat, 
            MondayTime = settings.MondayTime.ToString(@"hh\:mm"), UseMondayTime = settings.UseMondayTime, OtherDays = new List<string>(new []
        {
            settings.TuesdayTime.ToString(@"hh\:mm"),
            settings.WednesdayTime.ToString(@"hh\:mm"),
            settings.ThursdayTime.ToString(@"hh\:mm"),
            settings.FridayTime.ToString(@"hh\:mm")
        })});
    }
    
    [Authorize]
    [ValidateApiSubscription]
    [HttpGet("gp")]
    public async Task<IActionResult> GetProfile([FromServices] AppContext appContext)
    {
        var accessToken = Request.Headers[HeaderNames.Authorization];

        var id = _jwtService.ValidateToken(accessToken.ToString()).Claims.First().Value;

        var user = (await appContext.Users.FirstOrDefaultAsync(x => x.Id.ToString() == id))!;
        var profile = (await appContext.Profiles.FirstOrDefaultAsync(x => x.Id.ToString() == id))!;
        
        return Ok(new GetProfile { Username = profile.Username!, AvatarUrl = profile.AvatarUrl!, Group = user.Group!});
    }
    
    [Authorize]
    [ValidateApiSubscription]
    [HttpPost("saf")]
    public async Task<IActionResult> SaveAF([FromBody] SaveFormat format, [FromServices] AppContext appContext)
    {
        var accessToken = Request.Headers[HeaderNames.Authorization];
        var id = _jwtService.ValidateToken(accessToken.ToString()).Claims.First().Value;

        var settings = (await appContext.Settings.FirstOrDefaultAsync(x => x.Id.ToString() == id))!;
        
        settings.ASMessageFormat = format.Format;

        appContext.Settings.Update(settings);
        await appContext.SaveChangesAsync();
        
        return Ok();
    }
    
    [Authorize]
    [ValidateApiSubscription]
    [HttpPost("sf")]
    public async Task<IActionResult> SaveF([FromBody] SaveFormat format, [FromServices] AppContext appContext)
    {
        var accessToken = Request.Headers[HeaderNames.Authorization];

        var id = _jwtService.ValidateToken(accessToken.ToString()).Claims.First().Value;
        
        var settings = (await appContext.Settings.FirstOrDefaultAsync(x => x.Id.ToString() == id))!;
        
        settings.GPSMessageFormat = format.Format;

        appContext.Settings.Update(settings);
        await appContext.SaveChangesAsync();
        
        return Ok();
    }
    
    [Authorize]
    [ValidateApiSubscription]
    [HttpPost("se")]
    public async Task<IActionResult> SaveEtc([FromBody] SaveEtc etc, [FromServices] AppContext appContext)
    {
        var accessToken = Request.Headers[HeaderNames.Authorization];

        var id = _jwtService.ValidateToken(accessToken.ToString()).Claims.First().Value;
        var settings = (await appContext.Settings.FirstOrDefaultAsync(x => x.Id.ToString() == id))!;

        settings.UseMondayTime = etc.UseMondayTime;
        settings.MondayTime = TimeSpan.ParseExact(etc.MondayTime, @"hh\:mm", CultureInfo.InvariantCulture);
        settings.TuesdayTime = TimeSpan.ParseExact(etc.OtherDays[0], @"hh\:mm", CultureInfo.InvariantCulture);
        settings.WednesdayTime = TimeSpan.ParseExact(etc.OtherDays[1], @"hh\:mm", CultureInfo.InvariantCulture);
        settings.ThursdayTime = TimeSpan.ParseExact(etc.OtherDays[2], @"hh\:mm", CultureInfo.InvariantCulture);
        settings.FridayTime = TimeSpan.ParseExact(etc.OtherDays[3], @"hh\:mm", CultureInfo.InvariantCulture);

        settings.ItemFormat = etc.ItemFormat;

        appContext.Settings.Update(settings);
        await appContext.SaveChangesAsync();

        return Ok();
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginReq loginReq, [FromServices] AppContext appContext)
    {
        /*var botToken = _botConfiguration.BotToken;
        
        var loginWidget = new LoginWidget(botToken);

        var auth = loginWidget.CheckAuthorization(new SortedDictionary<string, string>
        {
            {"id",loginReq.Id},
            {"first_name", loginReq.FirstName},
            {"username", loginReq.Username},
            {"photo_url", loginReq.PhotoUrl},
            {"auth_date", loginReq.AuthDate},
            {"hash", loginReq.Hash}
        });

        if (auth != Authorization.Valid) return BadRequest(new {Error = auth});*/
        var token = _jwtService.GenerateToken(loginReq.Id);

        var subscriptionItem = await appContext.Subscriptions.FirstOrDefaultAsync(x => x
            .Id.ToString() == loginReq.Id);

        if (subscriptionItem == null) return BadRequest();
        
        var item = await appContext.Profiles.FirstOrDefaultAsync(x => x.Id.ToString() == loginReq.Id);
        if (item == null)
        {
            item = new ProfileEntity {Id = Convert.ToInt64(loginReq.Id), Username = loginReq.Username, AvatarUrl = loginReq.PhotoUrl};

            await appContext.Profiles.AddAsync(item);
        }
        else
        {
            item.AvatarUrl = loginReq.PhotoUrl;
            item.Username = loginReq.Username;
            appContext.Profiles.Update(item);

        }

        await appContext.SaveChangesAsync();
        
        return Ok(new LoginRes {Token = token});
    }
}
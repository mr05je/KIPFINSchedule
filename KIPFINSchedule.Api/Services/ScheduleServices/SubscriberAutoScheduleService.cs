using KIPFINSchedule.Core.Parser;
using KIPFINSchedule.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using AppContext = KIPFINSchedule.Database.AppContext;
using ILogger = Serilog.ILogger;
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

namespace KIPFINSchedule.Api.Services.ScheduleServices;

public class SubscriberAutoScheduleService : IHostedService
{
    private AppContext _appContext;
    private readonly ILogger _logger;
    private readonly ScheduleParser _schedule;
    private readonly ITelegramBotClient _botClient;

    public SubscriberAutoScheduleService(IServiceScopeFactory serviceScopeFactory)
    {
        using var scope = serviceScopeFactory.CreateScope();
        
        _botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger>();
        _appContext = scope.ServiceProvider.GetRequiredService<AppContext>();
        _schedule = scope.ServiceProvider.GetRequiredService<ScheduleParser>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("[SUB_AUTOSCHEDULE] starting!");
        
        Task.Run(async () =>
        {
            var timeToSleep = DateTime.Today.AddDays(1).AddMinutes(10) - DateTime.UtcNow.AddHours(3);

            _logger.Information("[SUB_AUTOSCHEDULE] Sleeping {timeToSleep}", timeToSleep.ToString(@"hh\:mm"));
            
            await Task.Delay(timeToSleep, cancellationToken);
            
            while (cancellationToken.IsCancellationRequested)
            {
                _appContext = _appContext.GetNewInstance();
                var users = _appContext.Users.Include(x => x.Setting)
                    .Where(x => x.Subscription != null && x.AutoSchedule && x.Group != null);

                var settings = users.Select(x => new KeyValuePair<SettingEntity, string>(x.Setting!, x.Group!));

                var sorted = new List<SortedSettings>();

                var dateToSwitch = DateTime.Today;
                
                switch (dateToSwitch.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        sorted = settings.Select(x => new SortedSettings { Time = x.Key.MondayTime, Settings = x.Key, Group = x.Value })
                            .OrderBy(x => x.Time).ToList();
                        break;
                    case DayOfWeek.Tuesday:
                        sorted = settings.Select(x => new SortedSettings
                                { Time = x.Key.UseMondayTime ? x.Key.MondayTime : x.Key.TuesdayTime, Settings = x.Key, Group = x.Value })
                            .OrderBy(x => x.Time).ToList();
                        break;
                    case DayOfWeek.Wednesday:
                        sorted = settings.Select(x => new SortedSettings
                                { Time = x.Key.UseMondayTime ? x.Key.MondayTime : x.Key.WednesdayTime, Settings = x.Key, Group = x.Value })
                            .OrderBy(x => x.Time).ToList();
                        break;
                    case DayOfWeek.Thursday:
                        sorted = settings.Select(x => new SortedSettings
                                { Time = x.Key.UseMondayTime ? x.Key.MondayTime : x.Key.ThursdayTime, Settings = x.Key, Group = x.Value })
                            .OrderBy(x => x.Time).ToList();
                        break;
                    case DayOfWeek.Friday:
                        sorted = settings.Select(x => new SortedSettings
                                { Time = x.Key.UseMondayTime ? x.Key.MondayTime : x.Key.FridayTime, Settings = x.Key, Group = x.Value })
                            .OrderBy(x => x.Time).ToList();
                        break;
                    case DayOfWeek.Sunday:
                        timeToSleep = DateTime.Today.AddDays(1).AddMinutes(10) - DateTime.UtcNow.AddHours(3);
                        await Task.Delay(timeToSleep, cancellationToken);
                        continue;
                    case DayOfWeek.Saturday:
                        _logger.Information("!!!WEEKEND RESTART!!!");
                        Environment.FailFast("");
                        return;
                }

                var timeBeforeStart = DateTime.UtcNow.AddHours(3).Add(sorted.First().Time) - DateTime.UtcNow.AddHours(3);
                await Task.Delay(timeBeforeStart, cancellationToken);

                var lastSend = DateTime.UtcNow.AddHours(3);
                
                foreach (var sortedUser in sorted)
                {
                    var timeout = DateTime.Today.Add(sortedUser.Time) - lastSend;
                    await Task.Delay(timeout, cancellationToken);

                    try
                    {
                        await _botClient.SendTextMessageAsync(sortedUser.Settings.Id,
                            await _schedule.GenSchedule(sortedUser.Group,
                                sortedUser.Settings.ASMessageFormat ??
                                BaseMessageTemplate.BaseMessageText.Replace("{{header}}",
                                    sortedUser.Settings.Id > 0
                                        ? BaseMessageTemplate.ChatHeader
                                        : BaseMessageTemplate.ChannelHeader),
                                sortedUser.Settings.ItemFormat ?? BaseMessageTemplate.BaseItemFormat),
                            parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                    }
                    catch
                    {
                        _logger.Warning("Can't send message to chat id {chat_id}", sortedUser.Settings.Id);
                    }
                    
                    lastSend = DateTime.UtcNow.AddHours(3);
                }
            }
        }, cancellationToken);
    }

    public  Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class SortedSettings
{
    public TimeSpan Time { get; init; }
    
    public required SettingEntity Settings { get; init; }
    public required string Group { get; init; }
}
using KIPFINSchedule.Core.Parser;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using AppContext = KIPFINSchedule.Database.AppContext;
using ILogger = Serilog.ILogger;

namespace KIPFINSchedule.Api.Services.ScheduleServices;

public class AutoScheduleService : IHostedService
{
    private AppContext _appContext;
    private readonly ILogger _logger; 
    private readonly ScheduleParser _schedule;
    private readonly ITelegramBotClient _botClient;

    public AutoScheduleService(IServiceScopeFactory serviceScopeFactory)
    {
        using var scope = serviceScopeFactory.CreateScope();
        
        _botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger>();
        _appContext = scope.ServiceProvider.GetRequiredService<AppContext>();
        _schedule = scope.ServiceProvider.GetRequiredService<ScheduleParser>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("[AUTOSCHEDULE] starting!");

        Task.Run(async () =>
        {
            var timeToSleep = DateTime.Today.AddDays(1).AddMinutes(10) - DateTime.UtcNow.AddHours(3);
            
            _logger.Information("[AUTOSCHEDULE] Sleeping {timeToSleep}", timeToSleep.ToString(@"hh\:mm"));

            await Task.Delay(timeToSleep, cancellationToken);
            
            while (!cancellationToken.IsCancellationRequested)
            {
                _appContext = _appContext.GetNewInstance();

                var unsubUsers = _appContext.Users.Where(x => x.AutoSchedule == true && x.Subscription == null && x.Group != null);

                var timeout = DateTime.Today.AddHours(6) - DateTime.UtcNow.AddHours(3);

                await Task.Delay(timeout, cancellationToken);

                foreach (var unsubUser in unsubUsers)
                {
                    try
                    {
                        await _botClient.SendTextMessageAsync(unsubUser.Id,
                            await _schedule.GenSchedule(unsubUser.Group!,
                                BaseMessageTemplate.BaseMessageText.Replace("{{header}}",
                                    unsubUser.Id > 0
                                        ? BaseMessageTemplate.ChatHeader
                                        : BaseMessageTemplate.ChannelHeader),
                                BaseMessageTemplate.BaseItemFormat), parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken);
                    }
                    catch 
                    {
                        _logger.Warning("Can't send message to chat id {chat_id}", unsubUser.Id);
                    }
                }
            }
        }, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
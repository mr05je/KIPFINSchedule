using KIPFINSchedule.Core.Telegram.Inline.JsonData;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using AppContext = KIPFINSchedule.Database.AppContext;
using ILogger = Serilog.ILogger;

namespace KIPFINSchedule.Api.Services.UtilServices;

public class SubscriptionService : IHostedService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger _logger;
    private readonly AppContext _appContext;

    private bool _stopIsCalled; 
    
    public SubscriptionService(IServiceScopeFactory serviceScopeFactory)
    {
        using var scope = serviceScopeFactory.CreateScope();
        
        _botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger>();
        _appContext = scope.ServiceProvider.GetRequiredService<AppContext>();
    }

    private const string BaseExpiredSubscriptionText = 
        """
        Ваша подписка истекла!⌛
        Вы можете преобрести новую подписку нажав кнопку ниже⬇️
        """;

    private const string BaseSoonEndingSubscriptionText = 
        """
        Ваша подписка скоро истечёт!⌛
        Вы можете продлить её нажав кнопку ниже⬇️
        """;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            _logger.Information("[SUBSCRIPTION] starting");

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            while (!_stopIsCalled)
            {
                var expiredSubscriptions = _appContext.Subscriptions.Where(x => x.ExpireAt < DateTime.UtcNow);

                var subscriptionButton = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("💳Купить подписку",
                        JsonConvert.SerializeObject(
                            new InlineJson
                            {
                                Command = "BuySubscription",
                                JsonData = new JsonData()
                            }, Formatting.None, settings))
                };

                foreach (var expiredSubscription in expiredSubscriptions)
                {
                    _logger.Information("Sending message about subscription expire to chat id: {chat}",
                        expiredSubscription.Id);
                    await _botClient.SendTextMessageAsync(expiredSubscription.Id, BaseExpiredSubscriptionText,
                        replyMarkup: new InlineKeyboardMarkup(subscriptionButton),
                        cancellationToken: cancellationToken);
                }

                await expiredSubscriptions.ExecuteDeleteAsync(cancellationToken: cancellationToken);

                subscriptionButton = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("💳Продлить подписку",
                        JsonConvert.SerializeObject(
                            new InlineJson
                            {
                                Command = "BuySubscription",
                                JsonData = new JsonData()
                            }, Formatting.None, settings))
                };

                var soonEndingSubscriptions =
                    _appContext.Subscriptions.Where(x => x.ExpireAt < DateTime.UtcNow.AddDays(3) && !x.Notified);

                foreach (var soonEndingSubscription in soonEndingSubscriptions)
                {
                    _logger.Information("Sending message about soon ending subscription to chat id: {chat}",
                        soonEndingSubscription.Id);
                    await _botClient.SendTextMessageAsync(soonEndingSubscription.Id, BaseSoonEndingSubscriptionText,
                        replyMarkup: new InlineKeyboardMarkup(subscriptionButton),
                        cancellationToken: cancellationToken);
                    soonEndingSubscription.Notified = true;
                    _appContext.Update(soonEndingSubscription);
                }

                await _appContext.SaveChangesAsync(cancellationToken);

                await Task.Delay(10000, cancellationToken);
            }
        }, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _stopIsCalled = true;
        await Task.Delay(1000, cancellationToken);
        _logger.Information("Disposing Subscription service");
        
        await _appContext.DisposeAsync();
    }
}
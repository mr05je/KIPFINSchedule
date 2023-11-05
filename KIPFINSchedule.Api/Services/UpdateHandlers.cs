using KIPFINSchedule.Core.Parser;
using KIPFINSchedule.Core.Parser.Bypass;
using KIPFINSchedule.Core.Telegram.Commands;
using KIPFINSchedule.Core.Telegram.Inline;
using KIPFINSchedule.Core.Telegram.Inline.JsonData;
using KIPFINSchedule.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;
using AppContext = KIPFINSchedule.Database.AppContext;
using Invoice = KIPFINSchedule.Core.Telegram.Invoices.Invoice;
using Profile = KIPFINSchedule.Core.Telegram.Inline.Profile;

namespace KIPFINSchedule.Api.Services;

public class UpdateHandlers
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandlers> _logger;
    private readonly string _paymentProviderToken;
    private readonly AppContext _appContext;
    private readonly ScheduleParser _scheduleParser;
    
    private const string BaseScheduleFormat = 
        """
        Вот твоё расписание на {{date}}📚:
        {{schedule}}
        """;
    private const string BaseItemFormat = 
        "**{{audience}}** ___{{teacher}}___ ";

    public UpdateHandlers(ITelegramBotClient botClient, ILogger<UpdateHandlers> logger, IConfiguration configuration, AppContext appContext, ScheduleParser scheduleParser)
    {
        _botClient = botClient;
        _logger = logger;
        _appContext = appContext;
        _scheduleParser = scheduleParser;
        _paymentProviderToken = configuration["PaymentProviderToken"]!;
    }

    private Task HandleErrorAsync(Exception exception)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            { ChannelPost: { } channelPost } => BotOnMessageReceived(channelPost, cancellationToken),
            { Message: { } message } => BotOnMessageReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
            { PreCheckoutQuery: { } preCheckoutQuery } => BotOnPreCheckOutReceived(preCheckoutQuery, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update)
        };

        try
        {
            await handler;
        }
        catch (Exception e)
        {
            await HandleErrorAsync(e);
        }
    }

    private async Task BotOnPreCheckOutReceived(PreCheckoutQuery preCheckoutQuery, CancellationToken cancellationToken)
    {
        await _botClient.AnswerPreCheckoutQueryAsync(preCheckoutQuery.Id, cancellationToken);
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        if (message.SuccessfulPayment != null)
        {
            await BotOnPaymentSuccessfulReceived(message, cancellationToken);
            return;
        }
        
        if (string.IsNullOrEmpty(message.Text) || !message.Text.StartsWith('/') || message.Text.Length <= 1)
        {
            if (message.Chat.Type == ChatType.Private)
                await _botClient.SendTextMessageAsync(message.Chat.Id, "Я не смог распознать команду в твоём сообщение😓",
                    cancellationToken: cancellationToken);
            return;
        }

        var command = message.Text.Remove(0, 1).Split(' ', '@').First();

        var args = message.Text.Split(' ').Last();

        var isOwner = message.From?.Id == 756898545;
        var isGroup = message.Chat.Type == ChatType.Group;

        var subscriptionItem = await _appContext.Subscriptions.FirstOrDefaultAsync(x => x.Id == message.Chat.Id,
            cancellationToken: cancellationToken);
        
        var user = await _appContext.Users.FirstOrDefaultAsync(x => x.Id == message.Chat.Id, cancellationToken);

        if (user == null)
        {
            user = new UserEntity
            {
                Id = message.Chat.Id,
                AutoSchedule = false
            };
            
            await _appContext.Users.AddAsync(user, cancellationToken);
            await _appContext.SaveChangesAsync(cancellationToken);
        }

        var settings = await _appContext.Settings.FirstOrDefaultAsync(x => x.Id == message.Chat.Id, cancellationToken);

        switch (command)
        {
            case "start":
                await _botClient.SendTextMessageAsync(message.Chat.Id, Help.GetHelp(isOwner, false, isGroup),
                    parseMode: ParseMode.MarkdownV2, cancellationToken: cancellationToken);
                return;
            case "help":
                await _botClient.SendTextMessageAsync(message.Chat.Id, Help.GetHelp(isOwner, false, isGroup),
                    parseMode: ParseMode.MarkdownV2, cancellationToken: cancellationToken);
                return;
            case "contacts":
                await _botClient.SendTextMessageAsync(message.Chat.Id, Contacts.GetContacts(),
                    parseMode: ParseMode.MarkdownV2, cancellationToken: cancellationToken);
                return;
            case "news":
                await _botClient.SendTextMessageAsync(message.Chat.Id, News.GetNews(),
                    parseMode: ParseMode.MarkdownV2, cancellationToken: cancellationToken);
                return;
            case "profile":
                await _botClient.MakeRequestAsync(
                    Profile.GetProfileMessage(message.Chat.Id, user.Group, user.AutoSchedule, subscriptionItem != null, subscriptionItem?.ExpireAt ?? default),
                    cancellationToken);
                return;
            case "subscription":
                await _botClient.MakeRequestAsync(Subscription.GetSubscriptionMessage(message.Chat.Id), cancellationToken);
                return;
            case "gs": 
                await _botClient.MakeRequestAsync(SelectCourse.GetSelectMessage(message.Chat.Id, "GS"), cancellationToken);
                return;
            case "gsp": 
                if (user.Group == null)
                {
                    await _botClient.MakeRequestAsync(SelectCourse.GetSelectMessage(message.Chat.Id, "GS"),
                        cancellationToken);
                    return;
                }

                await _botClient.SendTextMessageAsync(message.Chat.Id,
                    await _scheduleParser.GenSchedule(user.Group,
                        settings != null ? settings.GPSMessageFormat ?? BaseScheduleFormat : BaseScheduleFormat,
                        settings != null ? settings.ItemFormat ?? BaseItemFormat : BaseItemFormat),
                    parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                
                return;
            
            case "fr" when isOwner:
                await _botClient.SendTextMessageAsync(message.Chat.Id, "!!!restarting🔁!!!", cancellationToken: cancellationToken);
                
                Environment.FailFast("!!!RESTARTING!!!");
                
                return;
            
            case "set_link" when isOwner:
                KIPHttpClient.SetTempLink(args);

                await _botClient.SendTextMessageAsync(message.Chat.Id, $"Новая временная ссылка -> {args}", cancellationToken: cancellationToken);

                return;

            case "reset_link" when isOwner:
                KIPHttpClient.SetTempLink("");

                await _botClient.SendTextMessageAsync(message.Chat.Id, $"Используется основная ссылка", cancellationToken: cancellationToken);

                return;
        }
    }

    private async Task BotOnPaymentSuccessfulReceived(Message message, CancellationToken cancellationToken)
    {
        var days = message.SuccessfulPayment!.TotalAmount switch { 45000 => 300, 25000 => 180, 13500 => 90, _ => 90 };

        var item = await _appContext.Subscriptions.FirstOrDefaultAsync(x => x.Id == message.Chat.Id,
            cancellationToken: cancellationToken);
        
        if (item == null)
            await _appContext.Subscriptions.AddAsync(
                new SubscriptionEntity
                {
                    ExpireAt = DateTime.UtcNow.AddDays(days),
                    ProviderPaymentChargeId = message.SuccessfulPayment.ProviderPaymentChargeId,
                    Id = message.Chat.Id
                }, cancellationToken);
        else
        {
            item.ExpireAt = item.ExpireAt.AddDays(days);
            _appContext.Subscriptions.Update(item);
        }
        
        await _appContext.SaveChangesAsync(cancellationToken);

        await _botClient.SendTextMessageAsync(message.Chat.Id,
            SubscriptionSuccessful.GetSubscriptionSuccessful(item != null, item?.ExpireAt ?? default, days),
            cancellationToken: cancellationToken);
    }

    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Data == null) return;

        var data = JsonConvert.DeserializeObject<InlineJson>(callbackQuery.Data);
        if (data == null) return;

        if (data.JsonData.Step is Step.Exit)
        {
            try
            {
                await _botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, cancellationToken: cancellationToken);
            }
            catch
            {
                _logger.LogWarning("Can't delete message in chat id {chat_id} message id  {message_id}", callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId);
                throw;
            }
            
            return;
        }

        await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);

        var user = await _appContext.Users.FirstOrDefaultAsync(x => x.Id == callbackQuery.Message!.Chat.Id, cancellationToken);
        
        var subscriptionItem = await _appContext.Subscriptions
            .FirstOrDefaultAsync(x => x.Id == callbackQuery.Message!.Chat.Id, cancellationToken);
        
        switch (data.Command)
        {
            case "AutoSchedule":
                
                user!.AutoSchedule = !data.JsonData.Bool!.Value;

                _appContext.Users.Update(user);
                await _appContext.SaveChangesAsync(cancellationToken);
                
                await _botClient.MakeRequestAsync(Profile.GetProfileMessage(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, user.Group, user.AutoSchedule, subscriptionItem != null, subscriptionItem?.ExpireAt ?? default),
                    cancellationToken);
                return;
            case "BuySubscription":
                await _botClient.MakeRequestAsync(Subscription.GetSubscriptionMessage(callbackQuery.Message!.Chat.Id),
                    cancellationToken);
                return;
            case "SendSubscriptionInvoice":
                await _botClient.MakeRequestAsync(
                    Invoice.GetSubscriptionInvoice(callbackQuery.Message!.Chat.Id, _paymentProviderToken,
                        data.JsonData.Int switch { 3 => 135, 6 => 250, 10 => 450, _ => 135 }),
                    cancellationToken);
                return;
            case "PGS" when data.JsonData.Step == Step.Start:
                await _botClient.MakeRequestAsync(SelectCourse.GetSelectMessage(callbackQuery.Message!.Chat.Id, data.Command), cancellationToken);
                return;
            case "PGS" when data.JsonData.Step == Step.SelectCourse:
                await _botClient.MakeRequestAsync(SelectGroup.GetEditMessage(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, data.Command, data.JsonData.String!), cancellationToken);
                return;
            case "PGS" when data.JsonData.Step == Step.SelectGroup:
                user = await _appContext.Users.FirstOrDefaultAsync(x => x.Id == callbackQuery.Message!.Chat.Id, cancellationToken);

                user!.Group = data.JsonData.String;

                _appContext.Users.Update(user);
                await _appContext.SaveChangesAsync(cancellationToken);
                
                await _botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId,
                    cancellationToken: cancellationToken);
                
                await _botClient.MakeRequestAsync(
                    Profile.GetProfileMessage(callbackQuery.Message!.Chat.Id, user.Group, true, subscriptionItem != null, subscriptionItem?.ExpireAt ?? default),
                    cancellationToken);
                return;
            case "PGS" when data.JsonData.Step == Step.StepBack:
                await _botClient.MakeRequestAsync(SelectCourse.GetEditMessage(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, data.Command), cancellationToken);
                return;
            
            case "GS" when data.JsonData.Step == Step.SelectCourse:
                await _botClient.MakeRequestAsync(SelectGroup.GetEditMessage(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, data.Command, data.JsonData.String!), cancellationToken);
                return;
            case "GS" when data.JsonData.Step == Step.StepBack:
                await _botClient.MakeRequestAsync(SelectCourse.GetEditMessage(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, data.Command), cancellationToken);
                return;
            case "GS" when data.JsonData.Step == Step.SelectGroup:
                await _botClient.DeleteMessageAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId,
                    cancellationToken: cancellationToken);
                await _botClient.SendTextMessageAsync(callbackQuery.Message!.Chat.Id,
                    await _scheduleParser.GenSchedule(data.JsonData.String!, BaseScheduleFormat, BaseItemFormat),
                    parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                return;
        }
    }
    
    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}
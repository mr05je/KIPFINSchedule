using KIPFINSchedule.Core.Telegram.Inline.JsonData;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace KIPFINSchedule.Core.Telegram.Inline;

public static class Subscription
{
    private const string BaseText =
        @"Для оплаты выберите период, на который желаете преобрести подписку";

    public static SendMessageRequest GetSubscriptionMessage(long chatId)
    {
        var message = new SendMessageRequest(chatId, BaseText);
        
        var firstRow = new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData("450₽ за 300 дней",
                JsonExtension.SerializeObject(
                    new InlineJson
                    {
                        Command = "SendSubscriptionInvoice", 
                        JsonData = new JsonData.JsonData { Int = 10 }
                    }))
        };
        
        var secondRow = new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData("250₽ за 180 дней",
                JsonExtension.SerializeObject(
                    new InlineJson
                    {
                        Command = "SendSubscriptionInvoice",
                        JsonData = new JsonData.JsonData { Int = 6 }
                    })),
            InlineKeyboardButton.WithCallbackData("135₽ за 90 дней",
                JsonExtension.SerializeObject(
                    new InlineJson
                    {
                        Command = "SendSubscriptionInvoice",
                        JsonData = new JsonData.JsonData { Int = 3 }
                    }))
        };

        message.ReplyMarkup = new InlineKeyboardMarkup(new[]
        {
            firstRow, secondRow
        });

        message.ParseMode = ParseMode.MarkdownV2;

        return message;
    }
}
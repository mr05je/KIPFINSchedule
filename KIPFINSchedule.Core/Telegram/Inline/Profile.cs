using System.Globalization;
using KIPFINSchedule.Core.Telegram.Inline.JsonData;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace KIPFINSchedule.Core.Telegram.Inline;

public static class Profile
{
    private const string GroupBaseHeader = "ваш";
    private const string ChatBaseHeader = "твой";

    private const string ChatBaseText =
        """
        Привет! Вот {{header}} профиль👤:
        Группа👥: {{user_group}}, авторасписание⏰ {{auto_schedule}}
        Подписка на бота: {{has_subscription}}
        """;

    public static SendMessageRequest GetProfileMessage(long chatId,
        string? userGroup,
        bool autoSchedule,
        bool hasSubscription,
        DateTime expireAt = default,
        bool isChannel = false,
        bool isGroup = false)
    {
        var ci = new CultureInfo("ru-RU");

        var message = new SendMessageRequest(chatId,
            ChatBaseText
                .Replace("{{header}}", !isGroup && !isChannel ? ChatBaseHeader : GroupBaseHeader)
                .Replace("{{user_group}}", userGroup ?? "Не указана")
                .Replace("{{auto_schedule}}", autoSchedule ? "включено" : "выключено")
                .Replace("{{has_subscription}}",
                    hasSubscription ? "истекает " + expireAt.ToString("D", ci) : "отсутствует")
            .Replace(".", "\\.")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("-", "\\-")
            .Replace("!", "\\!")
            .Replace("?", "\\?"));

        var firstRow = new List<InlineKeyboardButton?>
        {
            InlineKeyboardButton.WithCallbackData($"⏰Авторасписание: {(autoSchedule ? "включено" : "выключено")}",
                JsonExtension.SerializeObject(
                    new InlineJson
                    {
                        Command = "AutoSchedule",
                        JsonData = new JsonData.JsonData { Bool = autoSchedule }
                    }))
        };

        InlineKeyboardButton? extendSubscription = null;
        InlineKeyboardButton subscription;

        if (hasSubscription)
        {
            subscription = InlineKeyboardButton.WithUrl($"⌛Подписка валидна до {expireAt.ToString("D", ci)}",
                "https://kip.mr05je.ru");
            extendSubscription = InlineKeyboardButton.WithCallbackData("💳Продлить подписку",
                JsonExtension.SerializeObject(
                    new InlineJson
                    {
                        Command = "BuySubscription",
                        JsonData = new JsonData.JsonData()
                    })
                );
        }
        else
            subscription = InlineKeyboardButton.WithCallbackData("💳Купить подписку",
                JsonExtension.SerializeObject(
                    new InlineJson
                    {
                        Command = "BuySubscription",
                        JsonData = new JsonData.JsonData()
                    })
                );

        var secondRow = new List<InlineKeyboardButton?> { subscription };

        var thirdRow = new List<InlineKeyboardButton?>
        {
            InlineKeyboardButton.WithCallbackData($"⚙️Настройка группы",
                JsonExtension.SerializeObject(new InlineJson
                {
                    Command = "PGS",
                    JsonData = new JsonData.JsonData
                    {
                        Step = Step.Start
                    }
                }))
        };

        if (!hasSubscription)
        {
            message.ReplyMarkup = new InlineKeyboardMarkup(new[]
            {
                firstRow,
                secondRow,
                thirdRow
            }!);
        }
        else
        {
            message.ReplyMarkup = new InlineKeyboardMarkup(new[]
            {
                firstRow,
                secondRow,
                new List<InlineKeyboardButton?> {extendSubscription},
                thirdRow
            }!);
        }

        message.ParseMode = ParseMode.MarkdownV2;

        return message;
    }

    public static EditMessageTextRequest GetProfileMessage(long chatId, int messageId, string? userGroup,
        bool autoSchedule,
        bool hasSubscription,
        DateTime expireAt = default,
        bool isChannel = false,
        bool isGroup = false)
    {
        var ci = new CultureInfo("ru-RU");

        var message = new EditMessageTextRequest(chatId, messageId,
            ChatBaseText
                .Replace("{{header}}", !isGroup && !isChannel ? ChatBaseHeader : GroupBaseHeader)
                .Replace("{{user_group}}", userGroup ?? "Не указана")
                .Replace("{{auto_schedule}}", autoSchedule ? "включено" : "выключено")
                .Replace("{{has_subscription}}",
                    hasSubscription ? "истекает " + expireAt.ToString("D", ci) : "отсутствует")
            .Replace(".", "\\.")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("-", "\\-")
            .Replace("!", "\\!")
            .Replace("?", "\\?"));

        var firstRow = new List<InlineKeyboardButton?>
        {
            InlineKeyboardButton.WithCallbackData($"⏰Авторасписание: {(autoSchedule ? "включено" : "выключено")}",
                JsonExtension.SerializeObject(
                    new InlineJson
                    {
                        Command = "AutoSchedule",
                        JsonData = new JsonData.JsonData { Bool = autoSchedule }
                    }))
        };

        InlineKeyboardButton? extendSubscription = null;
        InlineKeyboardButton subscription;

        if (hasSubscription)
        {
            subscription = InlineKeyboardButton.WithUrl($"⌛Подписка валидна до {expireAt.ToString("D", ci)}",
                "https://kip.mr05je.ru");
            extendSubscription = InlineKeyboardButton.WithCallbackData("💳Продлить подписку",
                JsonExtension.SerializeObject(
                    new InlineJson
                    {
                        Command = "BuySubscription",
                        JsonData = new JsonData.JsonData()
                    })
                );
        }
        else
            subscription = InlineKeyboardButton.WithCallbackData("💳Купить подписку",
                JsonExtension.SerializeObject(
                    new InlineJson
                    {
                        Command = "BuySubscription",
                        JsonData = new JsonData.JsonData()
                    })
                );

        var secondRow = new List<InlineKeyboardButton?> { subscription };

        var thirdRow = new List<InlineKeyboardButton?>
        {
            InlineKeyboardButton.WithCallbackData($"⚙️Настройка группы",
                JsonExtension.SerializeObject(new InlineJson
                {
                    Command = "PGS",
                    JsonData = new JsonData.JsonData
                    {
                        Step = Step.Start
                    }
                }))
        };

        if (!hasSubscription)
        {
            message.ReplyMarkup = new InlineKeyboardMarkup(new[]
            {
                firstRow,
                secondRow,
                thirdRow
            }!);
        }
        else
        {
            message.ReplyMarkup = new InlineKeyboardMarkup(new[]
            {
                firstRow,
                secondRow,
                new List<InlineKeyboardButton?> {extendSubscription},
                thirdRow
            }!);
        }

        message.ParseMode = ParseMode.MarkdownV2;

        return message;
    }
}
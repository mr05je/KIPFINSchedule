using KIPFINSchedule.Core.Telegram.Inline.JsonData;
using Newtonsoft.Json;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace KIPFINSchedule.Core.Telegram.Inline;

public static class SelectCourse
{
    private static readonly GroupModel Groups = JsonConvert.DeserializeObject<GroupModel>(File.ReadAllText(Environment.CurrentDirectory + "/Telegram/groups.json"))!;

    private const string BaseText =
        "Выберите ваш курс";

    public static SendMessageRequest GetSelectMessage(long chatId, string command)
    {
        var message = new SendMessageRequest(chatId, BaseText);

        var buttons = Groups.Groups!.Select(course => InlineKeyboardButton.WithCallbackData(course.Key,
            JsonExtension.SerializeObject(new InlineJson 
            { 
                Command = command,
                JsonData = new JsonData.JsonData { Step = Step.SelectCourse, String = course.Key }
            }))
        ).ToList();

        var exitButton = new List<InlineKeyboardButton> 
        { 
            InlineKeyboardButton.WithCallbackData("❌Отмена", 
                JsonExtension.SerializeObject(new InlineJson 
                { 
                    Command = "", 
                    JsonData = new JsonData.JsonData { Step = Step.Exit }
                }
                ))
        };

        message.ParseMode = ParseMode.MarkdownV2;
        
        message.ReplyMarkup = new InlineKeyboardMarkup(new[] { buttons, exitButton });

        return message;
    }
    
    public static EditMessageTextRequest GetEditMessage(long chatId, int messageId, string command)
    {
        var message = new EditMessageTextRequest(chatId, messageId, BaseText);

        var buttons = Groups.Groups!.Select(course => InlineKeyboardButton.WithCallbackData(course.Key,
            JsonExtension.SerializeObject(new InlineJson 
            { 
                Command = command,
                JsonData = new JsonData.JsonData { Step = Step.SelectCourse, String = course.Key }
            }))
        ).ToList();

        var exitButton = new List<InlineKeyboardButton> 
        { 
            InlineKeyboardButton.WithCallbackData("❌Отмена", 
                JsonExtension.SerializeObject(new InlineJson 
                    { 
                        Command = "", 
                        JsonData = new JsonData.JsonData { Step = Step.Exit }
                    }
                ))
        };

        message.ParseMode = ParseMode.MarkdownV2;
        
        message.ReplyMarkup = new InlineKeyboardMarkup(new[] { buttons, exitButton });
        
        return message;
    }
}
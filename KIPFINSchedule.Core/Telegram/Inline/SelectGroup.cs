using KIPFINSchedule.Core.Telegram.Inline.JsonData;
using Newtonsoft.Json;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace KIPFINSchedule.Core.Telegram.Inline;

public static class SelectGroup
{
    private static readonly GroupModel Groups = JsonConvert.DeserializeObject<GroupModel>(File.ReadAllText(Environment.CurrentDirectory + "/Telegram/groups.json"))!;

    private const string BaseText = 
        "Выберите вашу группу";

    public static EditMessageTextRequest GetEditMessage(long chatId, int messageId, string command, string selectedCourse)
    {
        var message = new EditMessageTextRequest(chatId, messageId, BaseText);

        var buttons = new List<List<InlineKeyboardButton>>();

        var count = 0;
        var temp = new List<InlineKeyboardButton>();
        
        foreach (var group in Groups.Groups![selectedCourse])
        {
            if (count == 3)
            {
                count = 0;
                buttons.Add(temp);

                temp = new List<InlineKeyboardButton>();
            }

            count++;

            temp.Add(InlineKeyboardButton.WithCallbackData(group,
                JsonExtension.SerializeObject(
                    new InlineJson
                    {
                        Command = command,
                        JsonData = new JsonData.JsonData
                        {
                            Step = Step.SelectGroup,
                            String = group
                        }
                    }))
            );
        }
        
        buttons.Add(temp);

        var footer = new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData("⬅️Назад", 
                JsonExtension.SerializeObject(new InlineJson
                { 
                    Command = command, 
                    JsonData = new JsonData.JsonData 
                        { Step = Step.StepBack } 
                })),
            InlineKeyboardButton.WithCallbackData("❌Отмена", 
                JsonExtension.SerializeObject(new InlineJson 
                { 
                    Command = "", 
                    JsonData = new JsonData.JsonData { Step = Step.Exit } 
                }))
        };

        buttons.Add(footer);
        
        message.ParseMode = ParseMode.MarkdownV2;

        message.ReplyMarkup = new InlineKeyboardMarkup(buttons);
        
        return message;
    }
}
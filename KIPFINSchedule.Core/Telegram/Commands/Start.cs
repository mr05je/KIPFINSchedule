namespace KIPFINSchedule.Core.Telegram.Commands;

public static class Start
{
    private const string BaseText = 
        """
        Привет\! Рад буду помочь в {{header}} в учёбе🥸\!
        Советую глянуть мои команды с помощью /help\!
        А так же советую подписаться на [канал](https://t.me/kipfinbotnews) моего автора, там будет информация об обновлениях\!
        """;

    private const string ChatHeader = "твоей";
    private const string ChannelHeader = "вашей";
    
    public static string GetStart(bool isGroup)
    {
        return BaseText.Replace("{{header}}", !isGroup ? ChatHeader : ChannelHeader);
    }
}
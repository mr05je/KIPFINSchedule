namespace KIPFINSchedule.Core.Telegram.Commands;

public static class Contacts
{
    private const string BaseText = @"📞Для связи с моим автором пиши @mr05je \(vk/telegram\)";

    public static string GetContacts()
    {
        return BaseText;
    }
}
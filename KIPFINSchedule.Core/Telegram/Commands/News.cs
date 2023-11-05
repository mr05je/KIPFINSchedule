namespace KIPFINSchedule.Core.Telegram.Commands;

public static class News
{
    private const string BaseText =
        "Чтобы следить за обновлениями и другими новостями подпишись на [канал](https://t.me/kipfinbotnews)";

    public static string GetNews()
    {
        return BaseText;
    }
}
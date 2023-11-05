using System.Globalization;

namespace KIPFINSchedule.Core.Telegram.Commands;

public static class SubscriptionSuccessful
{
    private const string SubscriptionExtended = 
        @"Ваша подписка была продлена до {{expire_at}}🎉";
    
    private const string SubscriptionBought = 
        @"Вы приобрели подписку на {{days}} дней🎉";

    public static string GetSubscriptionSuccessful(bool isExtend, DateTime expireAt = default, int days = 0)
    {
        var ci = new CultureInfo("ru-RU");

        return isExtend
            ? SubscriptionExtended.Replace("{{expire_at}}", expireAt.ToString("D", ci))
            : SubscriptionBought.Replace("{{days}}", days.ToString());
    }
}
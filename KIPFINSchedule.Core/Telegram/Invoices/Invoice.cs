using Telegram.Bot.Requests;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types.Payments;

namespace KIPFINSchedule.Core.Telegram.Invoices;

public static class Invoice
{
    private const string BaseTitle = "Подписка";
    private const string BaseText =
@"Для оплаты подписки на {{days}} дней нажмите кнопку ниже";

    public static SendInvoiceRequest GetSubscriptionInvoice(long chatId, string paymentProviderToken, int price)
    {
        var item = new List<LabeledPrice> { new($"К оплате {price}₽", price * 100) };

        var invoice = new SendInvoiceRequest(
            chatId, 
            BaseTitle,
            BaseText.Replace("{{days}}",
            price switch { 135 => "90", 250 => "180", 450 => "300", _ => "90" }), 
            "subscription",
            paymentProviderToken, 
            "RUB", 
            item);
        
        return invoice;
    }
}
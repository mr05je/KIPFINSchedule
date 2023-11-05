namespace KIPFINSchedule.Api.Services.ScheduleServices;

public static class BaseMessageTemplate
{
    public const string BaseMessageText = 
        """
        Привет! Вот {{header}} расписание на сегодня📚:
        {{schedule}}
        """;

    public const string ChatHeader = "твоё";
    public const string ChannelHeader = "ваше";
    
    public const string BaseItemFormat = 
        "**{{audience}}** ___{{teacher}}___ ";
}
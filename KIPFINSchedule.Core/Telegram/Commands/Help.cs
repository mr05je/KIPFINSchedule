// ReSharper disable UseRawString

namespace KIPFINSchedule.Core.Telegram.Commands;

public static class Help
{
    private const string ChatBaseHeader =
        @"Привет🧐\! Вот команды, доступные тебе:";

    private const string GroupBaseHeader =
        @"Рад быть в вашей группе 👻\! Вот команды, доступны вам:";

    private const string ChatBaseText =
        @"
/gsp \- получить расписание \(если указана группа в профиле, если нет \- смотри /gs\)📘
/gs \- получить расписание с выбором конкретной группы📙
/profile \- настройка профиля🪪
/subscription \- купить или проверить статус подписки💳
/news \- ссылка на канал с новостями📝
/contacts \- контакты для связи👨‍💻";

    private const string AdminCommands =
        @"
/fr \- перезапуск бота🔧
/set\_link \- ставит временную ссылку на файл⏳
/reset\_link \- использует исходную ссылку на файл↩️";

    private const string ChannelBaseText =
        @"Рад быть в вашем канале😎\! Вот команды, доступны вам:
/gsp \- получить расписание \(если указана группа в профиле, если нет \- смотри gs\)📘
/gs \- получить расписание с выбором конкретной группы📙
/profile \- настройка профиля🪪
/news \- ссылка на канал с новостями📝
/contacts \- контакты для связи👨‍💻";

    public static string GetHelp(bool isAdmin = false, bool isChannel = false, bool isGroup = false)
    {
        return !isChannel
            ? !isGroup ? ChatBaseHeader + ChatBaseText + (isAdmin ? AdminCommands : "") : GroupBaseHeader + ChatBaseText
            : ChannelBaseText;
    }
}
namespace KIPFINSchedule.Api;

public static class AppExtension
{
    public static ControllerActionEndpointConventionBuilder MapBotWebhookRoute<T>(
        this IEndpointRouteBuilder endpoints,
        string route)
    {
        var controllerName = typeof(T).Name.Replace("Controller", "", StringComparison.Ordinal);
        var actionName = typeof(T).GetMethods()[0].Name;

        return endpoints.MapControllerRoute(
            "bot_webhook",
            route,
            new { controller = controllerName, action = actionName });
    }
}
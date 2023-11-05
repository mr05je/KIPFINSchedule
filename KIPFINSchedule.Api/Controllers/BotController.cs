using KIPFINSchedule.Api.Filters;
using KIPFINSchedule.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace KIPFINSchedule.Api.Controllers;

public class BotController : ControllerBase
{
    [HttpPost]
    [ValidateTelegramBot]
    public async Task<IActionResult> Post(
        [FromBody] Update update,
        [FromServices] UpdateHandlers handleUpdateService,
        CancellationToken cancellationToken)
    {
        await handleUpdateService.HandleUpdateAsync(update, cancellationToken);
        return Ok();
    }
}
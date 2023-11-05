using KIPFINSchedule.Api.Services;
using KIPFINSchedule.Api.Services.UtilServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using Org.BouncyCastle.Asn1.Ocsp;
using AppContext = KIPFINSchedule.Database.AppContext;

namespace KIPFINSchedule.Api.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class ValidateApiSubscriptionAttribute : TypeFilterAttribute
{

    public ValidateApiSubscriptionAttribute() : base(typeof(ValidateApiSubscriptionFilter))
    {
    }
    
    private class ValidateApiSubscriptionFilter : ActionFilterAttribute
    {
        private readonly AppContext _appContext;
        private readonly JwtService _jwtService;

        public ValidateApiSubscriptionFilter(AppContext appContext, JwtService jwtService)
        {
            _appContext = appContext;
            _jwtService = jwtService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var accessToken = context.HttpContext.Request.Headers[HeaderNames.Authorization];
            
            var id = _jwtService.ValidateToken(accessToken.ToString()).Claims.First().Value;

            if (!_appContext.Subscriptions.Any(x => x.Id.ToString() == id))
                context.Result = new ObjectResult("!!!Subscription expired!!!")
                {
                    StatusCode = 401
                };
        }
    }
}
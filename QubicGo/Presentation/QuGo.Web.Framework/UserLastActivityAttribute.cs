using QuGo.Core;
using QuGo.Core.Data;
using QuGo.Core.Infrastructure;
using QuGo.Services.Users;
using System;
using System.Web.Mvc;

namespace QuGo.Web.Framework
{
    public class UserLastActivityAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;

            //don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            //only GET requests
            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            var customer = workContext.CurrentUser;

            //update last activity date
            if (customer.LastActivityDateUtc.AddMinutes(1.0) < DateTime.UtcNow)
            {
                var customerService = EngineContext.Current.Resolve<IUserService>();
                customer.LastActivityDateUtc = DateTime.UtcNow;
                customerService.UpdateUser(customer);
            }
        }
    }
}

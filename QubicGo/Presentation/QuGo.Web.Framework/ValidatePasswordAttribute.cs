using QuGo.Core;
using QuGo.Core.Data;
using QuGo.Core.Infrastructure;
using QuGo.Services.Users;
using System;
using System.Web.Mvc;

namespace QuGo.Web.Framework
{
    /// <summary>
    /// Represents filter attribute to validate customer password expiration
    /// </summary>
    public class ValidatePasswordAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Called by the ASP.NET MVC framework before the action method executes
        /// </summary>
        /// <param name="filterContext">The filter context</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;

            //don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            var actionName = filterContext.ActionDescriptor.ActionName;
            if (string.IsNullOrEmpty(actionName) || actionName.Equals("ChangePassword", StringComparison.InvariantCultureIgnoreCase))
                return;

            var controllerName = filterContext.Controller.ToString();
            if (string.IsNullOrEmpty(controllerName) || controllerName.Equals("User", StringComparison.InvariantCultureIgnoreCase))
                return;

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            //get current customer
            var user = EngineContext.Current.Resolve<IWorkContext>().CurrentUser;

            //check password expiration
            if (user.PasswordIsExpired())
            {
                var changePasswordUrl = new UrlHelper(filterContext.RequestContext).RouteUrl("UserChangePassword");
                filterContext.Result = new RedirectResult(changePasswordUrl);
            }
        }
    }
}
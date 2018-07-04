using System;
using System.Web.Mvc;
using QuGo.Core;
using QuGo.Core.Data;
using QuGo.Core.Domain.Users;
using QuGo.Core.Infrastructure;
using QuGo.Services.Common;

namespace QuGo.Web.Framework
{
    public class ApplicationLastVisitedPageAttribute : ActionFilterAttribute
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

            var userSettings = EngineContext.Current.Resolve<UserSettings>();
            if (!userSettings.StoreLastVisitedPage)
                return;

            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            var pageUrl = webHelper.GetThisPageUrl(true);
            if (!String.IsNullOrEmpty(pageUrl))
            {
                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();

                var previousPageUrl = workContext.CurrentUser.GetAttribute<string>(SystemUserAttributeNames.LastVisitedPage);
                if (!pageUrl.Equals(previousPageUrl))
                {
                    genericAttributeService.SaveAttribute(workContext.CurrentUser, SystemUserAttributeNames.LastVisitedPage, pageUrl);
                }
            }
        }
    }
}

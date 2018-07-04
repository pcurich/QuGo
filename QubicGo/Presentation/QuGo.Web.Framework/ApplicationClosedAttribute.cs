using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuGo.Core;
using QuGo.Core.Data;
using QuGo.Core.Domain;
using QuGo.Core.Infrastructure;
using QuGo.Services.Security;
using QuGo.Services.Topics;

namespace QuGo.Web.Framework
{
    /// <summary>
    /// Application closed attribute
    /// </summary>
    public class ApplicationClosedAttribute : ActionFilterAttribute
    {
        private readonly bool _ignore;

        /// <summary>
        /// Ctor 
        /// </summary>
        /// <param name="ignore">Pass false in order to ignore this functionality for a certain action method</param>
        public ApplicationClosedAttribute(bool ignore = false)
        {
            this._ignore = ignore;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null)
                return;

            //search the solution by "[StoreClosed(true)]" keyword 
            //in order to find method available even when a store is closed
            if (_ignore)
                return;

            HttpRequestBase request = filterContext.HttpContext.Request;
            if (request == null)
                return;

            string actionName = filterContext.ActionDescriptor.ActionName;
            if (String.IsNullOrEmpty(actionName))
                return;

            string controllerName = filterContext.Controller.ToString();
            if (String.IsNullOrEmpty(controllerName))
                return;

            //don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            var storeInformationSettings = EngineContext.Current.Resolve<ApplicationInformationSettings>();
            if (!storeInformationSettings.ApplicationClosed)
                return;

            //topics accessible when a store is closed
            if (controllerName.Equals("QuGo.Web.Controllers.TopicController", StringComparison.InvariantCultureIgnoreCase) &&
                actionName.Equals("TopicDetails", StringComparison.InvariantCultureIgnoreCase))
            {
                var topicService = EngineContext.Current.Resolve<ITopicService>();
                var storeContext = EngineContext.Current.Resolve<ISysContext>();
                var allowedTopicIds = topicService.GetAllTopics(storeContext.CurrentApplication.Id)
                    .Where(t => t.AccessibleWhenStoreClosed)
                    .Select(t => t.Id)
                    .ToList();
                var requestedTopicId = filterContext.RouteData.Values["topicId"] as int?;
                if (requestedTopicId.HasValue && allowedTopicIds.Contains(requestedTopicId.Value))
                    return;
            }

            //access to a closed store?
            var permissionService = EngineContext.Current.Resolve<IPermissionService>();
            if (permissionService.Authorize(StandardPermissionProvider.AccessClosedStore))
                return;

            var storeClosedUrl = new UrlHelper(filterContext.RequestContext).RouteUrl("StoreClosed");
            filterContext.Result = new RedirectResult(storeClosedUrl);
        }
    }
}

using System;
using System.Linq;
using QuGo.Core;
using QuGo.Core.Domain.Applications;
using QuGo.Services.Applications;

namespace QuGo.Web.Framework
{
    /// <summary>
    /// Application context for web application
    /// </summary>
    public partial class WebApplicationContext : ISysContext
    {
        private readonly IApplicationService _applicationService;
        private readonly IWebHelper _webHelper;

        private Application _cachedApplication;

        public WebApplicationContext(IApplicationService applicationService, IWebHelper webHelper)
        {
            this._applicationService = applicationService;
            this._webHelper = webHelper;
        }

        /// <summary>
        /// Gets or sets the current store
        /// </summary>
        public virtual Application CurrentApplication
        {
            get
            {
                if (_cachedApplication != null)
                    return _cachedApplication;

                //ty to determine the current store by HTTP_HOST
                var host = _webHelper.ServerVariables("HTTP_HOST");
                var allApplications = _applicationService.GetAllApplications();
                var store = allApplications.FirstOrDefault(s => s.ContainsHostValue(host));

                if (store == null)
                {
                    //load the first found store
                    store = allApplications.FirstOrDefault();
                }
                if (store == null)
                    throw new Exception("No store could be loaded");

                _cachedApplication = store;
                return _cachedApplication;
            }
        }
    }
}

using System.Net;
using QuGo.Core;
using QuGo.Services.Tasks;

namespace QuGo.Services.Common
{
    /// <summary>
    /// Represents a task for keeping the site alive
    /// </summary>
    public partial class KeepAliveTask : ITask
    {
        private readonly ISysContext _applicationContext;

        public KeepAliveTask(ISysContext applicationContext)
        {
            this._applicationContext = applicationContext;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            string url = _applicationContext.CurrentApplication.Url + "keepalive/index";
            using (var wc = new WebClient())
            {
                wc.DownloadString(url);
            }
        }
    }
}

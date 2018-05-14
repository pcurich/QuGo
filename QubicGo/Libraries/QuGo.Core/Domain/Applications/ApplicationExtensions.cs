using QuGo.Core.Domain.Applications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuGo.Core.Domain.Applications
{
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Parse comma-separated Hosts
        /// </summary>
        /// <param name="store">Store</param>
        /// <returns>Comma-separated hosts</returns>
        public static string[] ParseHostValues(this Application application)
        {
            if (application == null)
                throw new ArgumentNullException("application");

            var parsedValues = new List<string>();
            if (!String.IsNullOrEmpty(application.Hosts))
            {
                string[] hosts = application.Hosts.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string host in hosts)
                {
                    var tmp = host.Trim();
                    if (!String.IsNullOrEmpty(tmp))
                        parsedValues.Add(tmp);
                }
            }
            return parsedValues.ToArray();
        }

        /// <summary>
        /// Indicates whether a store contains a specified host
        /// </summary>
        /// <param name="store">Store</param>
        /// <param name="host">Host</param>
        /// <returns>true - contains, false - no</returns>
        public static bool ContainsHostValue(this Application application, string host)
        {
            if (application == null)
                throw new ArgumentNullException("application");

            if (String.IsNullOrEmpty(host))
                return false;

            var contains = application.ParseHostValues()
                                .FirstOrDefault(x => x.Equals(host, StringComparison.InvariantCultureIgnoreCase)) != null;
            return contains;
        }
    }
}

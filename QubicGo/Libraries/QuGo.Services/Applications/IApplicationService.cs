using System.Collections.Generic;
using QuGo.Core.Domain.Applications;

namespace QuGo.Services.Applications
{
    /// <summary>
    /// Application service interface
    /// </summary>
    public partial interface IApplicationService
    {
        /// <summary>
        /// Deletes a application
        /// </summary>
        /// <param name="application">Application</param>
        void DeleteApplication(Application application);

        /// <summary>
        /// Gets all applications
        /// </summary>
        /// <returns>Applications</returns>
        IList<Application> GetAllApplications();

        /// <summary>
        /// Gets a application 
        /// </summary>
        /// <param name="applicationId">Application identifier</param>
        /// <returns>Application</returns>
        Application GetApplicationById(int applicationId);

        /// <summary>
        /// Inserts a application
        /// </summary>
        /// <param name="application">Application</param>
        void InsertApplication(Application application);

        /// <summary>
        /// Updates the application
        /// </summary>
        /// <param name="application">Application</param>
        void UpdateApplication(Application application);
    }
}
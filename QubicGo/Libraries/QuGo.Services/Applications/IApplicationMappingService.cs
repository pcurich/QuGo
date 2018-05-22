using System.Collections.Generic;
using QuGo.Core;
using QuGo.Core.Domain.Applications;

namespace QuGo.Services.Applications
{
    /// <summary>
    /// Application mapping service interface
    /// </summary>i
    public partial interface IApplicationMappingService
    {
        /// <summary>
        /// Deletes a application mapping record
        /// </summary>
        /// <param name="applicationMapping">Application mapping record</param>
        void DeleteApplicationMapping(ApplicationMapping applicationMapping);

        /// <summary>
        /// Gets a application mapping record
        /// </summary>
        /// <param name="applicationMappingId">Application mapping record identifier</param>
        /// <returns>Application mapping record</returns>
        ApplicationMapping GetApplicationMappingById(int applicationMappingId);

        /// <summary>
        /// Gets application mapping records
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Application mapping records</returns>
        IList<ApplicationMapping> GetApplicationMappings<T>(T entity) where T : BaseEntity, IApplicationMappingSupported;

        /// <summary>
        /// Inserts a application mapping record
        /// </summary>
        /// <param name="applicationMapping">Application mapping</param>
        void InsertApplicationMapping(ApplicationMapping applicationMapping);

        /// <summary>
        /// Inserts a application mapping record
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="applicationId">Application id</param>
        /// <param name="entity">Entity</param>
        void InsertApplicationMapping<T>(T entity, int applicationId) where T : BaseEntity, IApplicationMappingSupported;

        /// <summary>
        /// Updates the application mapping record
        /// </summary>
        /// <param name="applicationMapping">Application mapping</param>
        void UpdateApplicationMapping(ApplicationMapping applicationMapping);

        /// <summary>
        /// Find application identifiers with granted access (mapped to the entity)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>Application identifiers</returns>
        int[] GetApplicationsIdsWithAccess<T>(T entity) where T : BaseEntity, IApplicationMappingSupported;

        /// <summary>
        /// Authorize whether entity could be accessed in the current application (mapped to this application)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize<T>(T entity) where T : BaseEntity, IApplicationMappingSupported;

        /// <summary>
        /// Authorize whether entity could be accessed in a application (mapped to this application)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="applicationId">Application identifier</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize<T>(T entity, int applicationId) where T : BaseEntity, IApplicationMappingSupported;
    }
}
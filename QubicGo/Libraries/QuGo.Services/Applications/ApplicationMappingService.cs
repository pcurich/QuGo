using System;
using System.Collections.Generic;
using System.Linq;
using QuGo.Core;
using QuGo.Core.Caching;
using QuGo.Core.Data;
using QuGo.Core.Domain.Catalog;
using QuGo.Core.Domain.Applications;
using QuGo.Services.Events;

namespace QuGo.Services.Applications
{
    /// <summary>
    /// Application mapping service
    /// </summary>
    public partial class ApplicationMappingService : IApplicationMappingService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : entity ID
        /// {1} : entity name
        /// </remarks>
        private const string APPLICATIONMAPPING_BY_ENTITYID_NAME_KEY = "QuGo.applicationmapping.entityid-name-{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string APPLICATIONMAPPING_PATTERN_KEY = "QuGo.applicationmapping.";

        #endregion

        #region Fields

        private readonly IRepository<ApplicationMapping> _applicationMappingRepository;
        private readonly ISysContext _sysContext;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
 
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="applicationContext">Application context</param>
        /// <param name="applicationMappingRepository">Application mapping repository</param>
        /// <param name="catalogSettings">Catalog settings</param>
        /// <param name="eventPublisher">Event publisher</param>
        public ApplicationMappingService(ICacheManager cacheManager,
            ISysContext sysContext,
            IRepository<ApplicationMapping> applicationMappingRepository, 
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._sysContext = sysContext;
            this._applicationMappingRepository = applicationMappingRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a application mapping record
        /// </summary>
        /// <param name="applicationMapping">Application mapping record</param>
        public virtual void DeleteApplicationMapping(ApplicationMapping applicationMapping)
        {
            if (applicationMapping == null)
                throw new ArgumentNullException("applicationMapping");

            _applicationMappingRepository.Delete(applicationMapping);

            //cache
            _cacheManager.RemoveByPattern(APPLICATIONMAPPING_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(applicationMapping);
        }

        /// <summary>
        /// Gets a application mapping record
        /// </summary>
        /// <param name="applicationMappingId">Application mapping record identifier</param>
        /// <returns>Application mapping record</returns>
        public virtual ApplicationMapping GetApplicationMappingById(int applicationMappingId)
        {
            if (applicationMappingId == 0)
                return null;

            return _applicationMappingRepository.GetById(applicationMappingId);
        }

        /// <summary>
        /// Gets application mapping records
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Application mapping records</returns>
        public virtual IList<ApplicationMapping> GetApplicationMappings<T>(T entity) where T : BaseEntity, IApplicationMappingSupported
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            var query = from sm in _applicationMappingRepository.Table
                        where sm.EntityId == entityId &&
                        sm.EntityName == entityName
                        select sm;
            var applicationMappings = query.ToList();
            return applicationMappings;
        }


        /// <summary>
        /// Inserts a application mapping record
        /// </summary>
        /// <param name="applicationMapping">Application mapping</param>
        public virtual void InsertApplicationMapping(ApplicationMapping applicationMapping)
        {
            if (applicationMapping == null)
                throw new ArgumentNullException("applicationMapping");

            _applicationMappingRepository.Insert(applicationMapping);

            //cache
            _cacheManager.RemoveByPattern(APPLICATIONMAPPING_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(applicationMapping);
        }

        /// <summary>
        /// Inserts a application mapping record
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="applicationId">Application id</param>
        /// <param name="entity">Entity</param>
        public virtual void InsertApplicationMapping<T>(T entity, int applicationId) where T : BaseEntity, IApplicationMappingSupported
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (applicationId == 0)
                throw new ArgumentOutOfRangeException("applicationId");

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            var applicationMapping = new ApplicationMapping
            {
                EntityId = entityId,
                EntityName = entityName,
                ApplicationId = applicationId
            };

            InsertApplicationMapping(applicationMapping);
        }

        /// <summary>
        /// Updates the application mapping record
        /// </summary>
        /// <param name="applicationMapping">Application mapping</param>
        public virtual void UpdateApplicationMapping(ApplicationMapping applicationMapping)
        {
            if (applicationMapping == null)
                throw new ArgumentNullException("applicationMapping");

            _applicationMappingRepository.Update(applicationMapping);

            //cache
            _cacheManager.RemoveByPattern(APPLICATIONMAPPING_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(applicationMapping);
        }

        /// <summary>
        /// Find application identifiers with granted access (mapped to the entity)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>Application identifiers</returns>
        public virtual int[] GetApplicationsIdsWithAccess<T>(T entity) where T : BaseEntity, IApplicationMappingSupported
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            string key = string.Format(APPLICATIONMAPPING_BY_ENTITYID_NAME_KEY, entityId, entityName);
            return _cacheManager.Get(key, () =>
            {
                var query = from sm in _applicationMappingRepository.Table
                            where sm.EntityId == entityId &&
                            sm.EntityName == entityName
                            select sm.ApplicationId;
                return query.ToArray();
            });
        }

        /// <summary>
        /// Authorize whether entity could be accessed in the current application (mapped to this application)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize<T>(T entity) where T : BaseEntity, IApplicationMappingSupported
        {
            return Authorize(entity, _sysContext.CurrentApplication.Id);
        }

        /// <summary>
        /// Authorize whether entity could be accessed in a application (mapped to this application)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="applicationId">Application identifier</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize<T>(T entity, int applicationId) where T : BaseEntity, IApplicationMappingSupported
        {
            if (entity == null)
                return false;

            if (applicationId == 0)
                //return true if no application specified/found
                return true;

            if (!entity.LimitedToApplications)
                return true;

            foreach (var applicationIdWithAccess in GetApplicationsIdsWithAccess(entity))
                if (applicationId == applicationIdWithAccess)
                    //yes, we have such permission
                    return true;

            //no permission found
            return false;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using QuGo.Core.Caching;
using QuGo.Core.Data;
using QuGo.Core.Domain.Applications;
using QuGo.Services.Events;

namespace QuGo.Services.Applications
{
    /// <summary>
    /// Application service
    /// </summary>
    public partial class ApplicationService : IApplicationService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string APPLICATIONS_ALL_KEY = "QuGo.application.all";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : application ID
        /// </remarks>
        private const string APPLICATIONS_BY_ID_KEY = "QuGo.application.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string APPLICATIONS_PATTERN_KEY = "QuGo.applications.";

        #endregion
        
        #region Fields
        
        private readonly IRepository<Application> _applicationRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="applicationRepository">Application repository</param>
        /// <param name="eventPublisher">Event published</param>
        public ApplicationService(ICacheManager cacheManager,
            IRepository<Application>  applicationRepository,
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._applicationRepository = applicationRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a application
        /// </summary>
        /// <param name="application">Application</param>
        public virtual void DeleteApplication(Application application)
        {
            if (application == null)
                throw new ArgumentNullException("application");

            var allApplications = GetAllApplications();
            if (allApplications.Count == 1)
                throw new Exception("You cannot delete the only configured application");

            _applicationRepository.Delete(application);

            _cacheManager.RemoveByPattern(APPLICATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(application);
        }

        /// <summary>
        /// Gets all applications
        /// </summary>
        /// <returns>Applications</returns>
        public virtual IList<Application> GetAllApplications()
        {
            string key = APPLICATIONS_ALL_KEY;
            return _cacheManager.Get(key, () =>
            {
                var query = from s in _applicationRepository.Table
                            orderby s.DisplayOrder, s.Id
                            select s;
                var applications = query.ToList();
                return applications;
            });
        }

        /// <summary>
        /// Gets a application 
        /// </summary>
        /// <param name="applicationId">Application identifier</param>
        /// <returns>Application</returns>
        public virtual Application GetApplicationById(int applicationId)
        {
            if (applicationId == 0)
                return null;
            
            string key = string.Format(APPLICATIONS_BY_ID_KEY, applicationId);
            return _cacheManager.Get(key, () => _applicationRepository.GetById(applicationId));
        }

        /// <summary>
        /// Inserts a application
        /// </summary>
        /// <param name="application">Application</param>
        public virtual void InsertApplication(Application application)
        {
            if (application == null)
                throw new ArgumentNullException("application");

            _applicationRepository.Insert(application);

            _cacheManager.RemoveByPattern(APPLICATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(application);
        }

        /// <summary>
        /// Updates the application
        /// </summary>
        /// <param name="application">Application</param>
        public virtual void UpdateApplication(Application application)
        {
            if (application == null)
                throw new ArgumentNullException("application");

            _applicationRepository.Update(application);

            _cacheManager.RemoveByPattern(APPLICATIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(application);
        }

        #endregion
    }
}
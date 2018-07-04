using System;
using System.Collections.Generic;
using System.Linq;
using QuGo.Core;
using QuGo.Core.Caching;
using QuGo.Core.Data;
using QuGo.Core.Domain.Catalog;
using QuGo.Core.Domain.Security;
using QuGo.Core.Domain.Applications;
using QuGo.Core.Domain.Topics;
using QuGo.Services.Users;
using QuGo.Services.Events;
using QuGo.Services.Applications;

namespace QuGo.Services.Topics
{
    /// <summary>
    /// Topic service
    /// </summary>
    public partial class TopicService : ITopicService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : application ID
        /// {1} : ignore ACL?
        /// {2} : show hidden?
        /// </remarks>
        private const string TOPICS_ALL_KEY = "QuGo.topics.all-{0}-{1}-{2}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : topic ID
        /// </remarks>
        private const string TOPICS_BY_ID_KEY = "QuGo.topics.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string TOPICS_PATTERN_KEY = "QuGo.topics.";

        #endregion
        
        #region Fields

        private readonly IRepository<Topic> _topicRepository;
        private readonly IRepository<ApplicationMapping> _applicationMappingRepository;
        private readonly IApplicationMappingService _applicationMappingService;
        private readonly IWorkContext _workContext;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly CatalogSettings _catalogSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        public TopicService(IRepository<Topic> topicRepository, 
            IRepository<ApplicationMapping> applicationMappingRepository,
            IApplicationMappingService applicationMappingService,
            IWorkContext workContext,
            IRepository<AclRecord> aclRepository,
            CatalogSettings catalogSettings,
            IEventPublisher eventPublisher,
            ICacheManager cacheManager)
        {
            this._topicRepository = topicRepository;
            this._applicationMappingRepository = applicationMappingRepository;
            this._applicationMappingService = applicationMappingService;
            this._workContext = workContext;
            this._aclRepository = aclRepository;
            this._catalogSettings = catalogSettings;
            this._eventPublisher = eventPublisher;
            this._cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        public virtual void DeleteTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Delete(topic);

            //cache
            _cacheManager.RemoveByPattern(TOPICS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(topic);
        }

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="topicId">The topic identifier</param>
        /// <returns>Topic</returns>
        public virtual Topic GetTopicById(int topicId)
        {
            if (topicId == 0)
                return null;

            string key = string.Format(TOPICS_BY_ID_KEY, topicId);
            return _cacheManager.Get(key, () => _topicRepository.GetById(topicId));
        }

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="systemName">The topic system name</param>
        /// <param name="applicationId">Store identifier; pass 0 to ignore filtering by application and load the first one</param>
        /// <returns>Topic</returns>
        public virtual Topic GetTopicBySystemName(string systemName, int applicationId = 0)
        {
            if (String.IsNullOrEmpty(systemName))
                return null;

            var query = _topicRepository.Table;
            query = query.Where(t => t.SystemName == systemName);
            query = query.OrderBy(t => t.Id);
            var topics = query.ToList();
            if (applicationId > 0)
            {
                topics = topics.Where(x => _applicationMappingService.Authorize(x, applicationId)).ToList();
            }
            return topics.FirstOrDefault();
        }

        /// <summary>
        /// Gets all topics
        /// </summary>
        /// <param name="applicationId">Store identifier; pass 0 to load all records</param>
        /// <param name="ignorAcl">A value indicating whether to ignore ACL rules</param>
        /// <param name="showHidden">A value indicating whether to show hidden topics</param>
        /// <returns>Topics</returns>
        public virtual IList<Topic> GetAllTopics(int applicationId, bool ignorAcl = false, bool showHidden = false)
        {
            string key = string.Format(TOPICS_ALL_KEY, applicationId, ignorAcl, showHidden);
            return _cacheManager.Get(key, () =>
            {
                var query = _topicRepository.Table;
                query = query.OrderBy(t => t.DisplayOrder).ThenBy(t => t.SystemName);

                if (!showHidden)
                    query = query.Where(t => t.Published);

                if ((applicationId > 0 && !_catalogSettings.IgnoreApplicationLimitations) ||
                    (!ignorAcl && !_catalogSettings.IgnoreAcl))
                {
                    if (!ignorAcl && !_catalogSettings.IgnoreAcl)
                    {
                        //ACL (access control list)
                        var allowedCustomerRolesIds = _workContext.CurrentUser.GetUserRoleIds();
                        query = from c in query
                                join acl in _aclRepository.Table
                                on new { c1 = c.Id, c2 = "Topic" } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into c_acl
                                from acl in c_acl.DefaultIfEmpty()
                                where !c.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.UserRoleId)
                                select c;
                    }
                    if (!_catalogSettings.IgnoreApplicationLimitations && applicationId > 0)
                    {
                        //Store mapping
                        query = from c in query
                                join sm in _applicationMappingRepository.Table
                                on new { c1 = c.Id, c2 = "Topic" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
                                from sm in c_sm.DefaultIfEmpty()
                                where !c.LimitedToApplications || applicationId == sm.ApplicationId
                                select c;
                    }

                    //only distinct topics (group by ID)
                    query = from t in query
                            group t by t.Id
                            into tGroup
                            orderby tGroup.Key
                            select tGroup.FirstOrDefault();
                    query = query.OrderBy(t => t.DisplayOrder).ThenBy(t => t.SystemName);
                }

                return query.ToList();                            
            });
        }

        /// <summary>
        /// Inserts a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        public virtual void InsertTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Insert(topic);

            //cache
            _cacheManager.RemoveByPattern(TOPICS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(topic);
        }

        /// <summary>
        /// Updates the topic
        /// </summary>
        /// <param name="topic">Topic</param>
        public virtual void UpdateTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Update(topic);

            //cache
            _cacheManager.RemoveByPattern(TOPICS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(topic);
        }

        #endregion
    }
}

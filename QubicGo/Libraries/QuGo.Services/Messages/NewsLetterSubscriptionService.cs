using System;
using System.Linq;
using QuGo.Core;
using QuGo.Core.Data;
using QuGo.Core.Domain.Users;
using QuGo.Core.Domain.Messages;
using QuGo.Data;
using QuGo.Services.Users;
using QuGo.Services.Events;

namespace QuGo.Services.Messages
{
    /// <summary>
    /// Newsletter subscription service
    /// </summary>
    public class NewsLetterSubscriptionService : INewsLetterSubscriptionService
    {
        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly IDbContext _context;
        private readonly IRepository<NewsLetterSubscription> _subscriptionRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IUserService _userService;

        #endregion

        #region Ctor

        public NewsLetterSubscriptionService(IDbContext context,
            IRepository<NewsLetterSubscription> subscriptionRepository,
            IRepository<User> userRepository,
            IEventPublisher eventPublisher,
            IUserService userService)
        {
            this._context = context;
            this._subscriptionRepository = subscriptionRepository;
            this._userRepository = userRepository;
            this._eventPublisher = eventPublisher;
            this._userService = userService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Publishes the subscription event.
        /// </summary>
        /// <param name="subscription">The newsletter subscription.</param>
        /// <param name="isSubscribe">if set to <c>true</c> [is subscribe].</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        private void PublishSubscriptionEvent(NewsLetterSubscription subscription, bool isSubscribe, bool publishSubscriptionEvents)
        {
            if (publishSubscriptionEvents)
            {
                if (isSubscribe)
                {
                    _eventPublisher.PublishNewsletterSubscribe(subscription);
                }
                else
                {
                    _eventPublisher.PublishNewsletterUnsubscribe(subscription);
                }
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Inserts a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        public virtual void InsertNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
            if (newsLetterSubscription == null)
            {
                throw new ArgumentNullException("newsLetterSubscription");
            }

            //Handle e-mail
            newsLetterSubscription.Email = CommonHelper.EnsureSubscriberEmailOrThrow(newsLetterSubscription.Email);

            //Persist
            _subscriptionRepository.Insert(newsLetterSubscription);

            //Publish the subscription event 
            if (newsLetterSubscription.Active)
            {
                PublishSubscriptionEvent(newsLetterSubscription, true, publishSubscriptionEvents);
            }

            //Publish event
            _eventPublisher.EntityInserted(newsLetterSubscription);
        }

        /// <summary>
        /// Updates a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        public virtual void UpdateNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
            if (newsLetterSubscription == null)
            {
                throw new ArgumentNullException("newsLetterSubscription");
            }

            //Handle e-mail
            newsLetterSubscription.Email = CommonHelper.EnsureSubscriberEmailOrThrow(newsLetterSubscription.Email);

            //Get original subscription record
            var originalSubscription = _context.LoadOriginalCopy(newsLetterSubscription);

            //Persist
            _subscriptionRepository.Update(newsLetterSubscription);

            //Publish the subscription event 
            if ((originalSubscription.Active == false && newsLetterSubscription.Active) ||
                (newsLetterSubscription.Active && (originalSubscription.Email != newsLetterSubscription.Email)))
            {
                //If the previous entry was false, but this one is true, publish a subscribe.
                PublishSubscriptionEvent(newsLetterSubscription, true, publishSubscriptionEvents);
            }
            
            if ((originalSubscription.Active && newsLetterSubscription.Active) && 
                (originalSubscription.Email != newsLetterSubscription.Email))
            {
                //If the two emails are different publish an unsubscribe.
                PublishSubscriptionEvent(originalSubscription, false, publishSubscriptionEvents);
            }

            if ((originalSubscription.Active && !newsLetterSubscription.Active))
            {
                //If the previous entry was true, but this one is false
                PublishSubscriptionEvent(originalSubscription, false, publishSubscriptionEvents);
            }

            //Publish event
            _eventPublisher.EntityUpdated(newsLetterSubscription);
        }

        /// <summary>
        /// Deletes a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        public virtual void DeleteNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
            if (newsLetterSubscription == null) throw new ArgumentNullException("newsLetterSubscription");

            _subscriptionRepository.Delete(newsLetterSubscription);

            //Publish the unsubscribe event 
            PublishSubscriptionEvent(newsLetterSubscription, false, publishSubscriptionEvents);

            //event notification
            _eventPublisher.EntityDeleted(newsLetterSubscription);
        }

        /// <summary>
        /// Gets a newsletter subscription by newsletter subscription identifier
        /// </summary>
        /// <param name="newsLetterSubscriptionId">The newsletter subscription identifier</param>
        /// <returns>NewsLetter subscription</returns>
        public virtual NewsLetterSubscription GetNewsLetterSubscriptionById(int newsLetterSubscriptionId)
        {
            if (newsLetterSubscriptionId == 0) return null;

            return _subscriptionRepository.GetById(newsLetterSubscriptionId);
        }

        /// <summary>
        /// Gets a newsletter subscription by newsletter subscription GUID
        /// </summary>
        /// <param name="newsLetterSubscriptionGuid">The newsletter subscription GUID</param>
        /// <returns>NewsLetter subscription</returns>
        public virtual NewsLetterSubscription GetNewsLetterSubscriptionByGuid(Guid newsLetterSubscriptionGuid)
        {
            if (newsLetterSubscriptionGuid == Guid.Empty) return null;

            var newsLetterSubscriptions = from nls in _subscriptionRepository.Table
                                          where nls.NewsLetterSubscriptionGuid == newsLetterSubscriptionGuid
                                          orderby nls.Id
                                          select nls;

            return newsLetterSubscriptions.FirstOrDefault();
        }

        /// <summary>
        /// Gets a newsletter subscription by email and application ID
        /// </summary>
        /// <param name="email">The newsletter subscription email</param>
        /// <param name="applicationId">Application identifier</param>
        /// <returns>NewsLetter subscription</returns>
        public virtual NewsLetterSubscription GetNewsLetterSubscriptionByEmailAndApplicationId(string email, int applicationId)
        {
            if (!CommonHelper.IsValidEmail(email)) 
                return null;

            email = email.Trim();

            var newsLetterSubscriptions = from nls in _subscriptionRepository.Table
                                          where nls.Email == email && nls.ApplicationId == applicationId
                                          orderby nls.Id
                                          select nls;

            return newsLetterSubscriptions.FirstOrDefault();
        }

        /// <summary>
        /// Gets the newsletter subscription list
        /// </summary>
        /// <param name="email">Email to search or string. Empty to load all records.</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="applicationId">Application identifier. 0 to load all records.</param>
        /// <param name="userRoleId">User role identifier. Used to filter subscribers by user role. 0 to load all records.</param>
        /// <param name="isActive">Value indicating whether subscriber record should be active or not; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>NewsLetterSubscription entities</returns>
        public virtual IPagedList<NewsLetterSubscription> GetAllNewsLetterSubscriptions(string email = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int applicationId = 0, bool? isActive = null, int userRoleId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (userRoleId == 0)
            {
                //do not filter by user role
                var query = _subscriptionRepository.Table;
                if (!String.IsNullOrEmpty(email))
                    query = query.Where(nls => nls.Email.Contains(email));
                if (createdFromUtc.HasValue)
                    query = query.Where(nls => nls.CreatedOnUtc >= createdFromUtc.Value);
                if (createdToUtc.HasValue)
                    query = query.Where(nls => nls.CreatedOnUtc <= createdToUtc.Value);
                if (applicationId > 0)
                    query = query.Where(nls => nls.ApplicationId == applicationId);
                if (isActive.HasValue)
                    query = query.Where(nls => nls.Active == isActive.Value);
                query = query.OrderBy(nls => nls.Email);

                var subscriptions = new PagedList<NewsLetterSubscription>(query, pageIndex, pageSize);
                return subscriptions;
            }
            else
            {
                //filter by user role
                var guestRole = _userService.GetUserRoleBySystemName(SystemUserRoleNames.Guests);
                if (guestRole == null)
                    throw new SysException("'Guests' role could not be loaded");

                if (guestRole.Id == userRoleId)
                {
                    //guests
                    var query = _subscriptionRepository.Table;
                    if (!String.IsNullOrEmpty(email))
                        query = query.Where(nls => nls.Email.Contains(email));
                    if (createdFromUtc.HasValue)
                        query = query.Where(nls => nls.CreatedOnUtc >= createdFromUtc.Value);
                    if (createdToUtc.HasValue)
                        query = query.Where(nls => nls.CreatedOnUtc <= createdToUtc.Value);
                    if (applicationId > 0)
                        query = query.Where(nls => nls.ApplicationId == applicationId);
                    if (isActive.HasValue)
                        query = query.Where(nls => nls.Active == isActive.Value);
                    query = query.Where(nls => !_userRepository.Table.Any(c => c.Email == nls.Email));
                    query = query.OrderBy(nls => nls.Email);
                    
                    var subscriptions = new PagedList<NewsLetterSubscription>(query, pageIndex, pageSize);
                    return subscriptions;
                }
                else
                {
                    //other user roles (not guests)
                    var query = _subscriptionRepository.Table.Join(_userRepository.Table,
                        nls => nls.Email,
                        c => c.Email,
                        (nls, c) => new
                        {
                            NewsletterSubscribers = nls,
                            User = c
                        });
                    query = query.Where(x => x.User.UserRoles.Any(cr => cr.Id == userRoleId));
                    if (!String.IsNullOrEmpty(email))
                        query = query.Where(x => x.NewsletterSubscribers.Email.Contains(email));
                    if (createdFromUtc.HasValue)
                        query = query.Where(x => x.NewsletterSubscribers.CreatedOnUtc >= createdFromUtc.Value);
                    if (createdToUtc.HasValue)
                        query = query.Where(x => x.NewsletterSubscribers.CreatedOnUtc <= createdToUtc.Value);
                    if (applicationId > 0)
                        query = query.Where(x => x.NewsletterSubscribers.ApplicationId == applicationId);
                    if (isActive.HasValue)
                        query = query.Where(x => x.NewsletterSubscribers.Active == isActive.Value);
                    query = query.OrderBy(x => x.NewsletterSubscribers.Email);

                    var subscriptions = new PagedList<NewsLetterSubscription>(query.Select(x=>x.NewsletterSubscribers), pageIndex, pageSize);
                    return subscriptions;
                }
            }
        }

        #endregion
    }
}
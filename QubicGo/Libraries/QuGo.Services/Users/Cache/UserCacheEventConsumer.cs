using QuGo.Core.Caching;
using QuGo.Core.Domain.Users;
using QuGo.Core.Infrastructure;
using QuGo.Services.Events;

namespace QuGo.Services.Users.Cache
{
    /// <summary>
    /// User cache event consumer (used for caching of current user password)
    /// </summary>
    public partial class UserCacheEventConsumer : IConsumer<UserPasswordChangedEvent>
    {
        #region Constants

        /// <summary>
        /// Key for current user password lifetime
        /// </summary>
        /// <remarks>
        /// {0} : user identifier
        /// </remarks>
        public const string USER_PASSWORD_LIFETIME = "QuGo.users.passwordlifetime-{0}";

        #endregion

        #region Fields

        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        public UserCacheEventConsumer()
        {
            //TODO inject static cache manager using constructor
            this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("qugo_cache_static");
        }

        #endregion

        #region Methods

        //password changed
        public void HandleEvent(UserPasswordChangedEvent eventMessage)
        {
            _cacheManager.Remove(string.Format(USER_PASSWORD_LIFETIME, eventMessage.Password.UserId));
        }

        #endregion
    }
}

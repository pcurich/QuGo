using QuGo.Core.Domain.Users;
using QuGo.Core.Domain.Directory;
using QuGo.Core.Domain.Localization;


namespace QuGo.Core
{
    /// <summary>
    /// Work context
    /// </summary>
    public interface IWorkContext
    {
        /// <summary>
        /// Gets or sets the current user
        /// </summary>
        User CurrentUser { get; set; }
        /// <summary>
        /// Gets or sets the original user (in case the current one is impersonated)
        /// </summary>
        User OriginalUserIfImpersonated { get; }
        /// <summary>
        /// Get or set current user working language
        /// </summary>
        Language WorkingLanguage { get; set; }
        /// <summary>
        /// Get or set current user working currency
        /// </summary>
        Currency WorkingCurrency { get; set; }
        /// <summary>
        /// Get or set value indicating whether we're in admin area
        /// </summary>
        bool IsAdmin { get; set; }
    }
}

using System;
using System.Linq;
using System.Web;
using QuGo.Core;
using QuGo.Core.Domain.Users;
using QuGo.Core.Domain.Directory;
using QuGo.Core.Domain.Localization;
using QuGo.Core.Domain.Tax;
using QuGo.Core.Domain.Vendors;
using QuGo.Core.Fakes;
using QuGo.Services.Authentication;
using QuGo.Services.Common;
using QuGo.Services.Users;
using QuGo.Services.Directory;
using QuGo.Services.Helpers;
using QuGo.Services.Localization;
using QuGo.Services.Applications;
using QuGo.Services.Vendors;
using QuGo.Web.Framework.Localization;

namespace QuGo.Web.Framework
{
    /// <summary>
    /// Work context for web application
    /// </summary>
    public partial class WebWorkContext : IWorkContext
    {
        #region Const

        private const string UserCookieName = "QuGo.user";

        #endregion

        #region Fields

        private readonly HttpContextBase _httpContext;
        private readonly IUserService _customerService;
        private readonly IVendorService _vendorService;
        private readonly ISysContext _sysContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly TaxSettings _taxSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IUserAgentHelper _userAgentHelper;
        private readonly IApplicationMappingService _storeMappingService;

        private User _cachedUser;
        private User _originalUserIfImpersonated;
        private Vendor _cachedVendor;
        private Language _cachedLanguage;
        private Currency _cachedCurrency;
        private TaxDisplayType? _cachedTaxDisplayType;

        #endregion

        #region Ctor

        public WebWorkContext(HttpContextBase httpContext,
            IUserService customerService,
            IVendorService vendorService,
            ISysContext storeContext,
            IAuthenticationService authenticationService,
            ILanguageService languageService,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            TaxSettings taxSettings, 
            CurrencySettings currencySettings,
            LocalizationSettings localizationSettings,
            IUserAgentHelper userAgentHelper,
            IApplicationMappingService storeMappingService)
        {
            this._httpContext = httpContext;
            this._customerService = customerService;
            this._vendorService = vendorService;
            this._sysContext = storeContext;
            this._authenticationService = authenticationService;
            this._languageService = languageService;
            this._currencyService = currencyService;
            this._genericAttributeService = genericAttributeService;
            this._taxSettings = taxSettings;
            this._currencySettings = currencySettings;
            this._localizationSettings = localizationSettings;
            this._userAgentHelper = userAgentHelper;
            this._storeMappingService = storeMappingService;
        }

        #endregion

        #region Utilities

        protected virtual HttpCookie GetUserCookie()
        {
            if (_httpContext == null || _httpContext.Request == null)
                return null;

            return _httpContext.Request.Cookies[UserCookieName];
        }

        protected virtual void SetUserCookie(Guid customerGuid)
        {
            if (_httpContext != null && _httpContext.Response != null)
            {
                var cookie = new HttpCookie(UserCookieName);
                cookie.HttpOnly = true;
                cookie.Value = customerGuid.ToString();
                if (customerGuid == Guid.Empty)
                {
                    cookie.Expires = DateTime.Now.AddMonths(-1);
                }
                else
                {
                    int cookieExpires = 24*365; //TODO make configurable
                    cookie.Expires = DateTime.Now.AddHours(cookieExpires);
                }

                _httpContext.Response.Cookies.Remove(UserCookieName);
                _httpContext.Response.Cookies.Add(cookie);
            }
        }

        protected virtual Language GetLanguageFromUrl()
        {
            if (_httpContext == null || _httpContext.Request == null)
                return null;

            string virtualPath = _httpContext.Request.AppRelativeCurrentExecutionFilePath;
            string applicationPath = _httpContext.Request.ApplicationPath;
            if (!virtualPath.IsLocalizedUrl(applicationPath, false))
                return null;

            var seoCode = virtualPath.GetLanguageSeoCodeFromUrl(applicationPath, false);
            if (String.IsNullOrEmpty(seoCode))
                return null;

            var language = _languageService
                .GetAllLanguages()
                .FirstOrDefault(l => seoCode.Equals(l.UniqueSeoCode, StringComparison.InvariantCultureIgnoreCase));
            if (language != null && language.Published && _storeMappingService.Authorize(language))
            {
                return language;
            }

            return null;
        }

        protected virtual Language GetLanguageFromBrowserSettings()
        {
            if (_httpContext == null ||
                _httpContext.Request == null ||
                _httpContext.Request.UserLanguages == null)
                return null;

            var userLanguage = _httpContext.Request.UserLanguages.FirstOrDefault();
            if (String.IsNullOrEmpty(userLanguage))
                return null;

            var language = _languageService
                .GetAllLanguages()
                .FirstOrDefault(l => userLanguage.Equals(l.LanguageCulture, StringComparison.InvariantCultureIgnoreCase));
            if (language != null && language.Published && _storeMappingService.Authorize(language))
            {
                return language;
            }

            return null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current user
        /// </summary>
        public virtual User CurrentUser
        {
            get
            {
                if (_cachedUser != null)
                    return _cachedUser;

                User user = null;
                if (_httpContext == null || _httpContext is FakeHttpContext)
                {
                    //check whether request is made by a background task
                    //in this case return built-in user record for background task
                    user = _customerService.GetUserBySystemName(SystemUserNames.BackgroundTask);
                }

                //check whether request is made by a search engine
                //in this case return built-in user record for search engines 
                //or comment the following two lines of code in order to disable this functionality
                if (user == null || user.Deleted || !user.Active || user.RequireReLogin)
                {
                    if (_userAgentHelper.IsSearchEngine())
                    {
                        user = _customerService.GetUserBySystemName(SystemUserNames.SearchEngine);
                    }
                }

                //registered user
                if (user == null || user.Deleted || !user.Active || user.RequireReLogin)
                {
                    user = _authenticationService.GetAuthenticatedUser();
                }

                //impersonate user if required (currently used for 'phone order' support)
                if (user != null && !user.Deleted && user.Active && !user.RequireReLogin)
                {
                    var impersonatedUserId = user.GetAttribute<int?>(SystemUserAttributeNames.ImpersonatedUserId);
                    if (impersonatedUserId.HasValue && impersonatedUserId.Value > 0)
                    {
                        var impersonatedUser = _customerService.GetUserById(impersonatedUserId.Value);
                        if (impersonatedUser != null && !impersonatedUser.Deleted && impersonatedUser.Active && !impersonatedUser.RequireReLogin)
                        {
                            //set impersonated user
                            _originalUserIfImpersonated = user;
                            user = impersonatedUser;
                        }
                    }
                }

                //load guest user
                if (user == null || user.Deleted || !user.Active || user.RequireReLogin)
                {
                    var customerCookie = GetUserCookie();
                    if (customerCookie != null && !String.IsNullOrEmpty(customerCookie.Value))
                    {
                        Guid customerGuid;
                        if (Guid.TryParse(customerCookie.Value, out customerGuid))
                        {
                            var customerByCookie = _customerService.GetUserByGuid(customerGuid);
                            if (customerByCookie != null &&
                                //this user (from cookie) should not be registered
                                !customerByCookie.IsRegistered())
                                user = customerByCookie;
                        }
                    }
                }

                //create guest if not exists
                if (user == null || user.Deleted || !user.Active || user.RequireReLogin)
                {
                    user = _customerService.InsertGuestUser();
                }


                //validation
                if (!user.Deleted && user.Active && !user.RequireReLogin)
                {
                    SetUserCookie(user.UserGuid);
                    _cachedUser = user;
                }

                return _cachedUser;
            }
            set
            {
                SetUserCookie(value.UserGuid);
                _cachedUser = value;
            }
        }

        /// <summary>
        /// Gets or sets the original user (in case the current one is impersonated)
        /// </summary>
        public virtual User OriginalUserIfImpersonated
        {
            get
            {
                return _originalUserIfImpersonated;
            }
        }

        /// <summary>
        /// Gets or sets the current vendor (logged-in manager)
        /// </summary>
        public virtual Vendor CurrentVendor
        {
            get
            {
                if (_cachedVendor != null)
                    return _cachedVendor;

                var currentUser = this.CurrentUser;
                if (currentUser == null)
                    return null;

                var vendor = _vendorService.GetVendorById(currentUser.VendorId);

                //validation
                if (vendor != null && !vendor.Deleted && vendor.Active)
                    _cachedVendor = vendor;

                return _cachedVendor;
            }
        }

        /// <summary>
        /// Get or set current user working language
        /// </summary>
        public virtual Language WorkingLanguage
        {
            get
            {
                if (_cachedLanguage != null)
                    return _cachedLanguage;
                
                Language detectedLanguage = null;
                if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                {
                    //get language from URL
                    detectedLanguage = GetLanguageFromUrl();
                }
                if (detectedLanguage == null && _localizationSettings.AutomaticallyDetectLanguage)
                {
                    //get language from browser settings
                    //but we do it only once
                    if (!this.CurrentUser.GetAttribute<bool>(SystemUserAttributeNames.LanguageAutomaticallyDetected, 
                        _genericAttributeService, _sysContext.CurrentApplication.Id))
                    {
                        detectedLanguage = GetLanguageFromBrowserSettings();
                        if (detectedLanguage != null)
                        {
                            _genericAttributeService.SaveAttribute(this.CurrentUser, SystemUserAttributeNames.LanguageAutomaticallyDetected,
                                 true, _sysContext.CurrentApplication.Id);
                        }
                    }
                }
                if (detectedLanguage != null)
                {
                    //the language is detected. now we need to save it
                    if (this.CurrentUser.GetAttribute<int>(SystemUserAttributeNames.LanguageId,
                        _genericAttributeService, _sysContext.CurrentApplication.Id) != detectedLanguage.Id)
                    {
                        _genericAttributeService.SaveAttribute(this.CurrentUser, SystemUserAttributeNames.LanguageId,
                            detectedLanguage.Id, _sysContext.CurrentApplication.Id);
                    }
                }

                var allLanguages = _languageService.GetAllLanguages(applicationId: _sysContext.CurrentApplication.Id);
                //find current user language
                var languageId = this.CurrentUser.GetAttribute<int>(SystemUserAttributeNames.LanguageId,
                    _genericAttributeService, _sysContext.CurrentApplication.Id);
                var language = allLanguages.FirstOrDefault(x => x.Id == languageId);
                if (language == null)
                {
                    //it not found, then let's load the default currency for the current language (if specified)
                    languageId = _sysContext.CurrentApplication.DefaultLanguageId;
                    language = allLanguages.FirstOrDefault(x => x.Id == languageId);
                }
                if (language == null)
                {
                    //it not specified, then return the first (filtered by current store) found one
                    language = allLanguages.FirstOrDefault();
                }
                if (language == null)
                {
                    //it not specified, then return the first found one
                    language = _languageService.GetAllLanguages().FirstOrDefault();
                }

                //cache
                _cachedLanguage = language;
                return _cachedLanguage;
            }
            set
            {
                var languageId = value != null ? value.Id : 0;
                _genericAttributeService.SaveAttribute(this.CurrentUser,
                    SystemUserAttributeNames.LanguageId,
                    languageId, _sysContext.CurrentApplication.Id);

                //reset cache
                _cachedLanguage = null;
            }
        }

        /// <summary>
        /// Get or set current user working currency
        /// </summary>
        public virtual Currency WorkingCurrency
        {
            get
            {
                if (_cachedCurrency != null)
                    return _cachedCurrency;
                
                //return primary store currency when we're in admin area/mode
                if (this.IsAdmin)
                {
                    var primaryApplicationCurrency =  _currencyService.GetCurrencyById(_currencySettings.PrimaryApplicationCurrencyId);
                    if (primaryApplicationCurrency != null)
                    {
                        //cache
                        _cachedCurrency = primaryApplicationCurrency;
                        return primaryApplicationCurrency;
                    }
                }

                var allCurrencies = _currencyService.GetAllCurrencies(applicationId: _sysContext.CurrentApplication.Id);
                //find a currency previously selected by a user
                var currencyId = this.CurrentUser.GetAttribute<int>(SystemUserAttributeNames.CurrencyId,
                    _genericAttributeService, _sysContext.CurrentApplication.Id);
                var currency = allCurrencies.FirstOrDefault(x => x.Id == currencyId);
                if (currency == null)
                {
                    //it not found, then let's load the default currency for the current language (if specified)
                    currencyId = this.WorkingLanguage.DefaultCurrencyId;
                    currency = allCurrencies.FirstOrDefault(x => x.Id == currencyId);
                }
                if (currency == null)
                {
                    //it not found, then return the first (filtered by current store) found one
                    currency = allCurrencies.FirstOrDefault();
                }
                if (currency == null)
                {
                    //it not specified, then return the first found one
                    currency = _currencyService.GetAllCurrencies().FirstOrDefault();
                }

                //cache
                _cachedCurrency = currency;
                return _cachedCurrency;
            }
            set
            {
                var currencyId = value != null ? value.Id : 0;
                _genericAttributeService.SaveAttribute(this.CurrentUser,
                    SystemUserAttributeNames.CurrencyId,
                    currencyId, _sysContext.CurrentApplication.Id);

                //reset cache
                _cachedCurrency = null;
            }
        }

        /// <summary>
        /// Get or set current tax display type
        /// </summary>
        public virtual TaxDisplayType TaxDisplayType
        {
            get
            {
                //cache
                if (_cachedTaxDisplayType != null)
                    return _cachedTaxDisplayType.Value;

                TaxDisplayType taxDisplayType;
                if (_taxSettings.AllowUsersToSelectTaxDisplayType && this.CurrentUser != null)
                {
                    taxDisplayType = (TaxDisplayType) this.CurrentUser.GetAttribute<int>(
                        SystemUserAttributeNames.TaxDisplayTypeId,
                        _genericAttributeService,
                        _sysContext.CurrentApplication.Id);
                }
                else
                {
                    taxDisplayType = _taxSettings.TaxDisplayType;
                }

                //cache
                _cachedTaxDisplayType = taxDisplayType;
                return _cachedTaxDisplayType.Value;

            }
            set
            {
                if (!_taxSettings.AllowUsersToSelectTaxDisplayType)
                    return;

                _genericAttributeService.SaveAttribute(this.CurrentUser, 
                    SystemUserAttributeNames.TaxDisplayTypeId,
                    (int)value, _sysContext.CurrentApplication.Id);

                //reset cache
                _cachedTaxDisplayType = null;

            }
        }

        /// <summary>
        /// Get or set value indicating whether we're in admin area
        /// </summary>
        public virtual bool IsAdmin { get; set; }

        #endregion
    }
}

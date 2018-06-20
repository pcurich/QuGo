using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QuGo.Core;
using QuGo.Core.Domain.Catalog;
using QuGo.Core.Domain.Users;
using QuGo.Core.Domain.Directory;
using QuGo.Core.Domain.Messages;
using QuGo.Services.Common;
using QuGo.Services.Users;
using QuGo.Services.Directory;
using QuGo.Services.Events;
using QuGo.Services.Helpers;
using QuGo.Services.Localization;
using QuGo.Services.Applications;
using QuGo.Core.Domain.Applications;
using QuGo.Core.Domain;

namespace QuGo.Services.Messages
{
    public partial class MessageTokenProvider : IMessageTokenProvider
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkContext _workContext;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IUserAttributeFormatter _userAttributeFormatter;
        private readonly IApplicationService _applicationService;
        private readonly ISysContext _sysContext;

        private readonly MessageTemplatesSettings _templatesSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;

        private readonly IEventPublisher _eventPublisher;
        private readonly ApplicationInformationSettings _applicationInformationSettings;

        #endregion

        #region Ctor

        public MessageTokenProvider(ILanguageService languageService,
            ILocalizationService localizationService, 
            IDateTimeHelper dateTimeHelper,
            ICurrencyService currencyService,
            IWorkContext workContext,
            IApplicationService applicationService,
            ISysContext sysContext,
            IAddressAttributeFormatter addressAttributeFormatter,
            IUserAttributeFormatter userAttributeFormatter,
            MessageTemplatesSettings templatesSettings,
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IEventPublisher eventPublisher,
            ApplicationInformationSettings applicationInformationSettings)
        {
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._currencyService = currencyService;
            this._workContext = workContext;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._userAttributeFormatter = userAttributeFormatter;
            this._applicationService = applicationService;
            this._sysContext = sysContext;

            this._templatesSettings = templatesSettings;
            this._catalogSettings = catalogSettings;
            this._currencySettings = currencySettings;
            this._eventPublisher = eventPublisher;
            this._applicationInformationSettings = applicationInformationSettings;
        }

        #endregion

        #region Allowed tokens

        private Dictionary<string, IEnumerable<string>> _allowedTokens;
        /// <summary>
        /// Get all available tokens by token groups
        /// </summary>
        protected Dictionary<string, IEnumerable<string>> AllowedTokens
        {
            get
            {
                if (_allowedTokens != null)
                    return _allowedTokens;

                _allowedTokens = new Dictionary<string, IEnumerable<string>>();

                //application tokens
                _allowedTokens.Add(TokenGroupNames.ApplicationTokens, new[]
                {
                    "%Application.Name%",
                    "%Application.URL%",
                    "%Application.Email%",
                    "%Application.CompanyName%",
                    "%Application.CompanyAddress%",
                    "%Application.CompanyPhoneNumber%",
                    "%Application.CompanyVat%",
                    "%Facebook.URL%",
                    "%Twitter.URL%",
                    "%YouTube.URL%",
                    "%GooglePlus.URL%"
                });

                //user tokens
                _allowedTokens.Add(TokenGroupNames.UserTokens, new[]
                {
                    "%User.Email%",
                    "%User.Username%",
                    "%User.FullName%",
                    "%User.FirstName%",
                    "%User.LastName%",
                    "%User.VatNumber%",
                    "%User.VatNumberStatus%",
                    "%User.CustomAttributes%",
                    "%User.PasswordRecoveryURL%",
                    "%User.AccountActivationURL%",
                    "%User.EmailRevalidationURL%",
                    "%Wishlist.URLForUser%"
                });

                //newsletter subscription tokens
                _allowedTokens.Add(TokenGroupNames.SubscriptionTokens, new[]
                {
                    "%NewsLetterSubscription.Email%",
                    "%NewsLetterSubscription.ActivationUrl%",
                    "%NewsLetterSubscription.DeactivationUrl%"
                });

                //contact us tokens
                _allowedTokens.Add(TokenGroupNames.ContactUs, new[]
                {
                    "%ContactUs.SenderEmail%",
                    "%ContactUs.SenderName%",
                    "%ContactUs.Body%"
                });

                return _allowedTokens;
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get application URL
        /// </summary>
        /// <param name="applicationId">Application identifier; Pass 0 to load URL of the current application</param>
        /// <returns></returns>
        protected virtual string GetApplicationUrl(int applicationId = 0)
        {
            var application = _applicationService.GetApplicationById(applicationId) ?? _sysContext.CurrentApplication;

            if (application == null)
                throw new Exception("No application could be loaded");

            return application.Url;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add application tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="application">Application</param>
        /// <param name="emailAccount">Email account</param>
        public virtual void AddApplicationTokens(IList<Token> tokens, Application application, EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            tokens.Add(new Token("Application.Name", application.GetLocalized(x => x.Name)));
            tokens.Add(new Token("Application.URL", application.Url, true));
            tokens.Add(new Token("Application.Email", emailAccount.Email));
            tokens.Add(new Token("Application.CompanyName", application.CompanyName));
            tokens.Add(new Token("Application.CompanyAddress", application.CompanyAddress));
            tokens.Add(new Token("Application.CompanyPhoneNumber", application.CompanyPhoneNumber));

            tokens.Add(new Token("Facebook.URL", _applicationInformationSettings.FacebookLink));
            tokens.Add(new Token("Twitter.URL", _applicationInformationSettings.TwitterLink));
            tokens.Add(new Token("YouTube.URL", _applicationInformationSettings.YoutubeLink));
            tokens.Add(new Token("GooglePlus.URL", _applicationInformationSettings.GooglePlusLink));

            //event notification
            _eventPublisher.EntityTokensAdded(application, tokens);
        }

        /// <summary>
        /// Add user tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="user">User</param>
        public virtual void AddUserTokens(IList<Token> tokens, User user)
        {
            tokens.Add(new Token("User.Email", user.Email));
            tokens.Add(new Token("User.Username", user.Username));
            tokens.Add(new Token("User.FullName", user.GetFullName()));
            tokens.Add(new Token("User.FirstName", user.GetAttribute<string>(SystemUserAttributeNames.FirstName)));
            tokens.Add(new Token("User.LastName", user.GetAttribute<string>(SystemUserAttributeNames.LastName)));
            tokens.Add(new Token("User.VatNumber", user.GetAttribute<string>(SystemUserAttributeNames.VatNumber)));

            var customAttributesXml = user.GetAttribute<string>(SystemUserAttributeNames.CustomUserAttributes);
            tokens.Add(new Token("User.CustomAttributes", _userAttributeFormatter.FormatAttributes(customAttributesXml), true));


            //note: we do not use SEO friendly URLS because we can get errors caused by having .(dot) in the URL (from the email address)
            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            var passwordRecoveryUrl = string.Format("{0}passwordrecovery/confirm?token={1}&email={2}", GetApplicationUrl(), user.GetAttribute<string>(SystemUserAttributeNames.PasswordRecoveryToken), HttpUtility.UrlEncode(user.Email));
            var accountActivationUrl = string.Format("{0}user/activation?token={1}&email={2}", GetApplicationUrl(), user.GetAttribute<string>(SystemUserAttributeNames.AccountActivationToken), HttpUtility.UrlEncode(user.Email));
            var emailRevalidationUrl = string.Format("{0}user/revalidateemail?token={1}&email={2}", GetApplicationUrl(), user.GetAttribute<string>(SystemUserAttributeNames.EmailRevalidationToken), HttpUtility.UrlEncode(user.Email));
            var wishlistUrl = string.Format("{0}wishlist/{1}", GetApplicationUrl(), user.UserGuid);

            tokens.Add(new Token("User.PasswordRecoveryURL", passwordRecoveryUrl, true));
            tokens.Add(new Token("User.AccountActivationURL", accountActivationUrl, true));
            tokens.Add(new Token("User.EmailRevalidationURL", emailRevalidationUrl, true));
            tokens.Add(new Token("Wishlist.URLForUser", wishlistUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(user, tokens);
        }

        /// <summary>
        /// Add newsletter subscription tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="subscription">Newsletter subscription</param>
        public virtual void AddNewsLetterSubscriptionTokens(IList<Token> tokens, NewsLetterSubscription subscription)
        {
            tokens.Add(new Token("NewsLetterSubscription.Email", subscription.Email));


            const string urlFormat = "{0}newsletter/subscriptionactivation/{1}/{2}";

            var activationUrl = String.Format(urlFormat, GetApplicationUrl(), subscription.NewsLetterSubscriptionGuid, "true");
            tokens.Add(new Token("NewsLetterSubscription.ActivationUrl", activationUrl, true));

            var deActivationUrl = String.Format(urlFormat, GetApplicationUrl(), subscription.NewsLetterSubscriptionGuid, "false");
            tokens.Add(new Token("NewsLetterSubscription.DeactivationUrl", deActivationUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(subscription, tokens);
        }

        /// <summary>
        /// Get collection of allowed (supported) message tokens for campaigns
        /// </summary>
        /// <returns>Collection of allowed (supported) message tokens for campaigns</returns>
        public virtual IEnumerable<string> GetListOfCampaignAllowedTokens()
        {
            var additionTokens = new CampaignAdditionTokensAddedEvent();
            _eventPublisher.Publish(additionTokens);

            var allowedTokens = GetListOfAllowedTokens(new[] { TokenGroupNames.ApplicationTokens, TokenGroupNames.SubscriptionTokens }).ToList();
            allowedTokens.AddRange(additionTokens.AdditionTokens);

            return allowedTokens.Distinct();
        }

        /// <summary>
        /// Get collection of allowed (supported) message tokens
        /// </summary>
        /// <param name="tokenGroups">Collection of token groups; pass null to get all available tokens</param>
        /// <returns>Collection of allowed message tokens</returns>
        public virtual IEnumerable<string> GetListOfAllowedTokens(IEnumerable<string> tokenGroups = null)
        {
            var additionTokens = new AdditionTokensAddedEvent();
            _eventPublisher.Publish(additionTokens);

            var allowedTokens = AllowedTokens.Where(x => tokenGroups == null || tokenGroups.Contains(x.Key))
                .SelectMany(x => x.Value).ToList();

            allowedTokens.AddRange(additionTokens.AdditionTokens);

            return allowedTokens.Distinct();
        }

        #endregion
    }
}

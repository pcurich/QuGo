using System.Collections.Generic;
using QuGo.Core.Domain.Messages;

namespace QuGo.Services.Messages
{
    /// <summary>
    /// Represents message template  extensions
    /// </summary>
    public static class MessageTemplateExtensions
    {
        /// <summary>
        /// Get token groups of message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <returns>Collection of token group names</returns>
        public static IEnumerable<string> GetTokenGroups(this MessageTemplate messageTemplate)
        {
            //groups depend on which tokens are added at the appropriate methods in IWorkflowMessageService
            switch (messageTemplate.Name)
            {
                case MessageTemplateSystemNames.UserRegisteredNotification:
                case MessageTemplateSystemNames.UserWelcomeMessage:
                case MessageTemplateSystemNames.UserEmailValidationMessage:
                case MessageTemplateSystemNames.UserEmailRevalidationMessage:
                case MessageTemplateSystemNames.UserPasswordRecoveryMessage:
                    return new[] { TokenGroupNames.ApplicationTokens, TokenGroupNames.UserTokens };
  
                case MessageTemplateSystemNames.NewsletterSubscriptionActivationMessage:
                case MessageTemplateSystemNames.NewsletterSubscriptionDeactivationMessage:
                    return new[] { TokenGroupNames.ApplicationTokens, TokenGroupNames.SubscriptionTokens };
 
                case MessageTemplateSystemNames.ContactUsMessage:
                    return new[] { TokenGroupNames.ApplicationTokens, TokenGroupNames.ContactUs };

 

                default:
                    return new string[] { };
            }
        }
    }
}
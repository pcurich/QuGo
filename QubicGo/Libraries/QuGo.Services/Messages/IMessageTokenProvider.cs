using System.Collections.Generic;
using QuGo.Core.Domain.Catalog;
using QuGo.Core.Domain.Users;
using QuGo.Core.Domain.Messages;
using QuGo.Core.Domain.Applications;


namespace QuGo.Services.Messages
{
    public partial interface IMessageTokenProvider
    {
        /// <summary>
        /// Add application tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="application">Application</param>
        /// <param name="emailAccount">Email account</param>
        void AddApplicationTokens(IList<Token> tokens, Application application, EmailAccount emailAccount);

        /// <summary>
        /// Add user tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="user">User</param>
        void AddUserTokens(IList<Token> tokens, User user);

        /// <summary>
        /// Add newsletter subscription tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="subscription">Newsletter subscription</param>
        void AddNewsLetterSubscriptionTokens(IList<Token> tokens, NewsLetterSubscription subscription);

        /// <summary>
        /// Get collection of allowed (supported) message tokens
        /// </summary>
        /// <param name="tokenGroups">Collection of token groups; pass null to get all available tokens</param>
        /// <returns>Collection of allowed message tokens</returns>
        IEnumerable<string> GetListOfAllowedTokens(IEnumerable<string> tokenGroups = null);
    }
}

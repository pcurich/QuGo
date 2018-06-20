using QuGo.Core;
using QuGo.Core.Domain.Common;

namespace QuGo.Services.Common
{
    /// <summary>
    /// Search term service interafce
    /// </summary>
    public partial interface ISearchTermService
    {
        /// <summary>
        /// Deletes a search term record
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        void DeleteSearchTerm(SearchTerm searchTerm);

        /// <summary>
        /// Gets a search term record by identifier
        /// </summary>
        /// <param name="searchTermId">Search term identifier</param>
        /// <returns>Search term</returns>
        SearchTerm GetSearchTermById(int searchTermId);

        /// <summary>
        /// Gets a search term record by keyword
        /// </summary>
        /// <param name="keyword">Search term keyword</param>
        /// <param name="applicationId">Store identifier</param>
        /// <returns>Search term</returns>
        SearchTerm GetSearchTermByKeyword(string keyword, int applicationId);

        /// <summary>
        /// Inserts a search term record
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        void InsertSearchTerm(SearchTerm searchTerm);

        /// <summary>
        /// Updates the search term record
        /// </summary>
        /// <param name="searchTerm">Search term</param>
         void UpdateSearchTerm(SearchTerm searchTerm);
    }
}
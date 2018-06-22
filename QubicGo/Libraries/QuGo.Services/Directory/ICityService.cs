using QuGo.Core.Domain.Directory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuGo.Services.Directory
{
    /// <summary>
    /// City service interface
    /// </summary>
    public partial interface ICityService
    {
        /// <summary>
        /// Delete a city
        /// </summary>
        /// <param name="city">The city</param>
        void DeleteCity(City city);

        /// <summary>
        /// Gets a city
        /// </summary>
        /// <param name="cityId">The city identifier</param>
        /// <returns>State/province</returns>
        City GetCityById(int cityId);

        /// <summary>
        /// Gets a city 
        /// </summary>
        /// <param name="abbreviation">The city abbreviation</param>
        /// <returns>city</returns>
        City GetCityByAbbreviation(string abbreviation);

        /// <summary>
        /// Gets a city collection by State/province identifier
        /// </summary>
        /// <param name="stateProvinceId">State/Province identifier</param>
        /// <param name="languageId">Language identifier. It's used to sort states by localized names (if specified); pass 0 to skip it</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>States</returns>
        IList<City> GetCityByStateProvinceId(int stateProvinceId, int languageId = 0, bool showHidden = false);

        /// <summary>
        /// Gets all states/provinces
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>States</returns>
        IList<City> GetCities(bool showHidden = false);

        /// <summary>
        /// Inserts a city
        /// </summary>
        /// <param name="city">State/province</param>
        void InsertCity(City city);

        /// <summary>
        /// Updates a city
        /// </summary>
        /// <param name="city">State/province</param>
        void UpdateCity(City city);
    }
}

using QuGo.Core.Caching;
using QuGo.Core.Data;
using QuGo.Core.Domain.Directory;
using QuGo.Services.Events;
using QuGo.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuGo.Services.Directory
{
    /// <summary>
    /// City service
    /// </summary>
    public class CityService : ICityService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : stateProvince ID
        /// {1} : language ID
        /// {2} : show hidden records?
        /// </remarks>
        private const string CITIES_ALL_KEY = "QuGo.city.all-{0}-{1}-{2}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CITIES_PATTERN_KEY = "QuGo.city.";

        #endregion

        #region Fields

        private readonly IRepository<City> _cityRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="cityRepository">City repository</param>
        /// <param name="eventPublisher">Event published</param>
        public CityService(ICacheManager cacheManager,
            IRepository<City> cityRepository,
            IEventPublisher eventPublisher)
        {
            _cacheManager = cacheManager;
            _cityRepository = cityRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a city
        /// </summary>
        /// <param name="city">The city</param>
        public virtual void DeleteCity(City city)
        {
            if (city == null)
                throw new ArgumentNullException("city");

            _cityRepository.Delete(city);

            _cacheManager.RemoveByPattern(CITIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(city);
        }

        /// <summary>
        /// Gets a city
        /// </summary>
        /// <param name="cityId">The city identifier</param>
        /// <returns>State/province</returns>
        public virtual City GetCityById(int cityId)
        {
            if (cityId == 0)
                return null;

            return _cityRepository.GetById(cityId);
        }

        /// <summary>
        /// Gets a city
        /// </summary>
        /// <param name="abbreviation">The city abbreviation</param>
        /// <returns>City</returns>
        public virtual City GetCityByAbbreviation(string abbreviation)
        {
            var query = from sp in _cityRepository.Table
                        where sp.Abbreviation == abbreviation
                        select sp;
            var city = query.FirstOrDefault();
            return city;
        }

        /// <summary>
        /// Gets a city collection by stateProvinces identifier
        /// </summary>
        /// <param name="stateProvinceId">state/Province identifier</param>
        /// <param name="languageId">Language identifier. It's used to sort states by localized names (if specified); pass 0 to skip it</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>States</returns>
        public virtual IList<City> GetCityByStateProvinceId(int stateProvinceId, int languageId = 0, bool showHidden = false)
        {
            string key = string.Format(CITIES_ALL_KEY, stateProvinceId, languageId, showHidden);
            return _cacheManager.Get(key, () =>
            {
                var query = from sp in _cityRepository.Table
                            orderby sp.DisplayOrder, sp.Name
                            where sp.StateProvinceId == stateProvinceId &&
                            (showHidden || sp.Published)
                            select sp;
                var cities = query.ToList();

                if (languageId > 0)
                {
                    //we should sort states by localized names when they have the same display order
                    cities = cities
                        .OrderBy(c => c.DisplayOrder)
                        .ThenBy(c => c.GetLocalized(x => x.Name, languageId))
                        .ToList();
                }
                return cities;
            });
        }

        /// <summary>
        /// Gets all cities
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>States</returns>
        public virtual IList<City> GetCities(bool showHidden = false)
        {
            var query = from sp in _cityRepository.Table
                        orderby sp.StateProvinceId, sp.DisplayOrder, sp.Name
                        where showHidden || sp.Published
                        select sp;
            var cities = query.ToList();
            return cities;
        }

        /// <summary>
        /// Inserts a city
        /// </summary>
        /// <param name="city">city</param>
        public virtual void InsertCity(City city)
        {
            if (city == null)
                throw new ArgumentNullException("city");

            _cityRepository.Insert(city);

            _cacheManager.RemoveByPattern(CITIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(city);
        }

        /// <summary>
        /// Updates a city
        /// </summary>
        /// <param name="city">city</param>
        public virtual void UpdateCity(City city)
        {
            if (city == null)
                throw new ArgumentNullException("city");

            _cityRepository.Insert(city);

            _cacheManager.RemoveByPattern(CITIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(city);
        }

        #endregion
    }
}

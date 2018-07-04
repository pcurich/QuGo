using System;
using System.Linq;
using QuGo.Core;
using QuGo.Core.Domain;
using QuGo.Core.Domain.Users;
using QuGo.Services.Common;

namespace QuGo.Web.Framework.Themes
{
    /// <summary>
    /// Theme context
    /// </summary>
    public partial class ThemeContext : IThemeContext
    {
        private readonly IWorkContext _workContext;
        private readonly ISysContext _sysContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ApplicationInformationSettings _applicationInformationSettings;
        private readonly IThemeProvider _themeProvider;

        private bool _themeIsCached;
        private string _cachedThemeName;

        public ThemeContext(IWorkContext workContext,
            ISysContext sysContext,
            IGenericAttributeService genericAttributeService,
            ApplicationInformationSettings applicationInformationSettings, 
            IThemeProvider themeProvider)
        {
            this._workContext = workContext;
            this._sysContext = sysContext;
            this._genericAttributeService = genericAttributeService;
            this._applicationInformationSettings = applicationInformationSettings;
            this._themeProvider = themeProvider;
        }

        /// <summary>
        /// Get or set current theme system name
        /// </summary>
        public string WorkingThemeName
        {
            get
            {
                if (_themeIsCached)
                    return _cachedThemeName;

                string theme = "";
                if (_applicationInformationSettings.AllowCustomerToSelectTheme)
                {
                    if (_workContext.CurrentUser != null)
                        theme = _workContext.CurrentUser.GetAttribute<string>(SystemUserAttributeNames.WorkingThemeName, _genericAttributeService, _sysContext.CurrentApplication.Id);
                }

                //default store theme
                if (string.IsNullOrEmpty(theme))
                    theme = _applicationInformationSettings.DefaultApplicationTheme;

                //ensure that theme exists
                if (!_themeProvider.ThemeConfigurationExists(theme))
                {
                    var themeInstance = _themeProvider.GetThemeConfigurations()
                        .FirstOrDefault();
                    if (themeInstance == null)
                        throw new Exception("No theme could be loaded");
                    theme = themeInstance.ThemeName;
                }
                
                //cache theme
                this._cachedThemeName = theme;
                this._themeIsCached = true;
                return theme;
            }
            set
            {
                if (!_applicationInformationSettings.AllowCustomerToSelectTheme)
                    return;

                if (_workContext.CurrentUser == null)
                    return;

                _genericAttributeService.SaveAttribute(_workContext.CurrentUser, SystemUserAttributeNames.WorkingThemeName, value, _sysContext.CurrentApplication.Id);

                //clear cache
                this._themeIsCached = false;
            }
        }
    }
}

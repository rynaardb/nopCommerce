using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.EFT.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.EFT.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class PaymentEFTController : BasePaymentController
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public PaymentEFTController(ILanguageService languageService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _languageService = languageService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var eftPaymentSettings = _settingService.LoadSetting<EFTPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                DescriptionText = eftPaymentSettings.DescriptionText
            };

            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.DescriptionText = _localizationService
                    .GetLocalizedSetting(eftPaymentSettings, x => x.DescriptionText, languageId, 0, false, false);
            });
            model.AdditionalFee = eftPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = eftPaymentSettings.AdditionalFeePercentage;
            model.ShippableProductRequired = eftPaymentSettings.ShippableProductRequired;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.DescriptionText_OverrideForStore = _settingService.SettingExists(eftPaymentSettings, x => x.DescriptionText, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(eftPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(eftPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.ShippableProductRequired_OverrideForStore = _settingService.SettingExists(eftPaymentSettings, x => x.ShippableProductRequired, storeScope);
            }

            return View("~/Plugins/Payments.EFT/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var eftPaymentSettings = _settingService.LoadSetting<EFTPaymentSettings>(storeScope);

            //save settings
            eftPaymentSettings.DescriptionText = model.DescriptionText;
            eftPaymentSettings.AdditionalFee = model.AdditionalFee;
            eftPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            eftPaymentSettings.ShippableProductRequired = model.ShippableProductRequired;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(eftPaymentSettings, x => x.DescriptionText, model.DescriptionText_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(eftPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(eftPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(eftPaymentSettings, x => x.ShippableProductRequired, model.ShippableProductRequired_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                _localizationService.SaveLocalizedSetting(eftPaymentSettings,
                    x => x.DescriptionText, localized.LanguageId, localized.DescriptionText);
            }

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        #endregion
    }
}
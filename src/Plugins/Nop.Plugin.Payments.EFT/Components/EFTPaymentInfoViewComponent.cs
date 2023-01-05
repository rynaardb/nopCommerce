using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.EFT.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.EFT.Components
{
    [ViewComponent(Name = "EFT")]
    public class EFTViewComponent : NopViewComponent
    {
        private readonly EFTPaymentSettings _eftPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public EFTViewComponent(EFTPaymentSettings eftPaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _eftPaymentSettings = eftPaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel
            {
                DescriptionText = _localizationService.GetLocalizedSetting(_eftPaymentSettings,
                    x => x.DescriptionText, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id)
            };

            return View("~/Plugins/Payments.EFT/Views/PaymentInfo.cshtml", model);
        }
    }
}
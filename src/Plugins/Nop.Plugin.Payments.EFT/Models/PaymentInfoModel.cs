using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.EFT.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        public string DescriptionText { get; set; }
    }
}
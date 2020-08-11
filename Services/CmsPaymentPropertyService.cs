using EPiServer;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Newtonsoft.Json;

namespace Foundation.Commerce.Payment.Alepay
{
    public interface ICmsPaymentPropertyService
    {
        ContentReference PaymentSettingPage { get; }
        string CheckoutPagePropertyName { get; }
        string PaymentPagePropertyName { get; }
        string OrderConfirmationPagePropertyName { get; }
        string AlepayCheckoutParams { get; }
        string GetCancelledPaymentUrl();
        string GetAcceptedPaymentUrl();
        string GetPayooPaymentProcessingPage();
        AlepayCheckoutParams GetCheckoutParams(IOrderGroup orderGroup);
    }

    [ServiceConfiguration(typeof(ICmsPaymentPropertyService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CmsPaymentPropertyService : ICmsPaymentPropertyService
    {
        public virtual ContentReference PaymentSettingPage => ContentReference.StartPage;
        public virtual string CheckoutPagePropertyName => "CheckoutPage";
        public virtual string PaymentPagePropertyName => "AlepayPaymentPage";
        public virtual string OrderConfirmationPagePropertyName => "OrderConfirmationPage";
        public virtual string AlepayCheckoutParams => "AlepayCheckoutParams";

        private readonly IContentLoader _contentLoader;
        private readonly UrlResolver _urlResolver;
        public CmsPaymentPropertyService(IContentLoader contentLoader, UrlResolver urlResolver)
        {
            _contentLoader = contentLoader;
            _urlResolver = urlResolver;
        }

        public virtual string GetCancelledPaymentUrl()
        {
            return this.GetUrlFromPaymentSettingPageProperty(CheckoutPagePropertyName);
        }

        public virtual string GetAcceptedPaymentUrl()
        {
            return this.GetUrlFromPaymentSettingPageProperty(OrderConfirmationPagePropertyName);
        }

        public virtual string GetPayooPaymentProcessingPage()
        {
            return this.GetUrlFromPaymentSettingPageProperty(PaymentPagePropertyName);
        }

        public virtual AlepayCheckoutParams GetCheckoutParams(IOrderGroup orderGroup)
        {
            if (!orderGroup.Properties.ContainsKey(AlepayCheckoutParams) || orderGroup.Properties[AlepayCheckoutParams] == null) 
                return new DefaultAlepayCheckoutParams();

            return JsonConvert.DeserializeObject<AlepayCheckoutParams>(orderGroup.Properties[AlepayCheckoutParams].ToString());
        }

        /// <summary>
        /// Gets url from start page's page reference property.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The friendly url.</returns>
        private string GetUrlFromPaymentSettingPageProperty(string propertyName)
        {
            var startPageData = _contentLoader.Get<PageData>(PaymentSettingPage);
            if (startPageData == null)
            {
                return _urlResolver.GetUrl(ContentReference.StartPage);
            }

            var contentLink = startPageData.Property[propertyName]?.Value as ContentReference;
            if (!ContentReference.IsNullOrEmpty(contentLink))
            {
                return _urlResolver.GetUrl(contentLink);
            }
            return _urlResolver.GetUrl(ContentReference.StartPage);
        }

    }
}

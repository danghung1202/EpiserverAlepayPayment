using System.Linq;
using System.Net;
using System.Web.Mvc;
using EPiServer.Commerce.Order;
using EPiServer.Editor;
using EPiServer.Logging.Compatibility;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Orders.Exceptions;

namespace Foundation.Commerce.Payment.Alepay
{
    public class AlepayPaymentController : PageController<AlepayPaymentPage>
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(AlepayPaymentController));

        private readonly IAlepayCartService _cartService;
        private readonly IAlepayClient _alepayClient;
        private readonly IOrderRepository _orderRepository;
        private readonly ICmsPaymentPropertyService _cmsPaymentPropertyService;

        public AlepayPaymentController(
            IAlepayCartService cartService,
            IAlepayClient alepayClient,
            IOrderRepository orderRepository,
            ICmsPaymentPropertyService cmsPaymentPropertyService)
        {
            _cartService = cartService;
            _alepayClient = alepayClient;
            _orderRepository = orderRepository;
            _cmsPaymentPropertyService = cmsPaymentPropertyService;
        }

        public ActionResult Index()
        {
            if (PageEditing.PageIsInEditMode)
            {
                return new EmptyResult();
            }
            var configuration = new AlepayConfiguration();
            var currentCart = _cartService.LoadDefaultCart();
            if (!currentCart.Forms.Any() || !currentCart.GetFirstForm().Payments.Any())
            {
                throw new PaymentException(PaymentException.ErrorType.ProviderError, "", "Generic Error");
            }

            var payment = currentCart.Forms.SelectMany(f => f.Payments).FirstOrDefault(c => c.PaymentMethodId.Equals(configuration.PaymentMethodId));
            if (payment == null)
            {
                throw new PaymentException(PaymentException.ErrorType.ProviderError, "", "Payment Not Specified");
            }

            var alepayResponseCallbackData = Request.QueryString["data"];
            if (string.IsNullOrEmpty(alepayResponseCallbackData)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            var transaction = _alepayClient.GetTransactionInfoFromResponseCallback(alepayResponseCallbackData);

            if (transaction == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            UpdatePaymentProperties(currentCart, payment, transaction);
            var purchaseOrder = _cartService.ProcessSuccessfulTransaction(currentCart, payment);
            if (purchaseOrder != null)
            {
                var confirmationOrderUrl = CreateAcceptRedirectionUrl(purchaseOrder);
                return Redirect(confirmationOrderUrl);
            }

            var redirectUrl = CreateCancelRedirectionUrl();
            var message = "Payment failed";
            TempData[Constant.ErrorMessages] = $"{message}";
            redirectUrl = _cartService.ProcessUnsuccessfulTransaction(redirectUrl, message);

            return Redirect(redirectUrl);
        }

        private void UpdatePaymentProperties(IOrderGroup orderGroup, IPayment payment, ResponseTransaction transaction)
        {
            payment.Properties[Constant.TransactionCode] = transaction.TransactionCode;
            payment.Properties[Constant.OrderCode] = transaction.OrderCode;
            payment.Properties[Constant.OrderAmount] = transaction.Amount;
            payment.Properties[Constant.Currency] = transaction.Currency;
            payment.Properties[Constant.TransactionStatus] = transaction.Status;
            payment.Properties[Constant.CardType] = transaction.Method;
            payment.Properties[Constant.CardNumber] = transaction.CardNumber;
            payment.Properties[Constant.Installment] = transaction.Installment;
            payment.Properties[Constant.Is3D] = transaction.Is3D;
            payment.Properties[Constant.Month] = transaction.Month;
            payment.Properties[Constant.BankCode] = transaction.BankCode;
            payment.Properties[Constant.BankName] = transaction.BankName;
            payment.Properties[Constant.TransactionTime] = transaction.TransactionTime;
            payment.Properties[Constant.SuccessTime] = transaction.SuccessTime;

            _orderRepository.Save(orderGroup);
        }

        private string CreateAcceptRedirectionUrl(IPurchaseOrder purchaseOrder)
        {
            var acceptUrl = _cmsPaymentPropertyService.GetAcceptedPaymentUrl();
            var redirectionUrl = UriUtil.AddQueryString(acceptUrl, "success", "true");
            redirectionUrl = UriUtil.AddQueryString(redirectionUrl, "contactId", purchaseOrder.CustomerId.ToString());
            redirectionUrl = UriUtil.AddQueryString(redirectionUrl, "orderNumber", purchaseOrder.OrderLink.OrderGroupId.ToString());

            return redirectionUrl;
        }

        private string CreateCancelRedirectionUrl()
        {
            var cancelUrl = _cmsPaymentPropertyService.GetCancelledPaymentUrl();
            cancelUrl = UriUtil.AddQueryString(cancelUrl, "success", "false");
            cancelUrl = UriUtil.AddQueryString(cancelUrl, "paymentmethod", "alepay");
            return cancelUrl;
        }
    }
}

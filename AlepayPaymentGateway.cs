using System;
using System.Linq;
using System.Web;
using EPiServer.Commerce.Order;
using EPiServer.Logging.Compatibility;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Plugins.Payment;

namespace Foundation.Commerce.Payment.Alepay
{
    public class AlepayPaymentGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(AlepayPaymentGateway));

        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly ICmsPaymentPropertyService _cmsPaymentPropertyService;
        private readonly IAlepayClient _alepayClient;

        public AlepayPaymentGateway()
            : this(
                ServiceLocator.Current.GetInstance<IAlepayClient>(),
                ServiceLocator.Current.GetInstance<IOrderNumberGenerator>(),
                ServiceLocator.Current.GetInstance<ICmsPaymentPropertyService>())
        { }

        public AlepayPaymentGateway(
            IAlepayClient alepayClient,
            IOrderNumberGenerator orderNumberGenerator,
            ICmsPaymentPropertyService cmsPaymentPropertyService)
        {
            _orderNumberGenerator = orderNumberGenerator;
            _cmsPaymentPropertyService = cmsPaymentPropertyService;
            _alepayClient = alepayClient;
        }

        /// <summary>
        /// Main entry point of ECF Payment Gateway.
        /// </summary>
        /// <param name="payment">The payment to process</param>
        /// <param name="message">The message.</param>
        /// <returns>return false and set the message will make the WorkFlow activity raise PaymentExcetion(message)</returns>
        public override bool ProcessPayment(Mediachase.Commerce.Orders.Payment payment, ref string message)
        {
            var orderGroup = payment.Parent.Parent;

            var paymentProcessingResult = ProcessPayment(orderGroup, payment);

            if (!string.IsNullOrEmpty(paymentProcessingResult.RedirectUrl))
            {
                HttpContext.Current.Response.Redirect(paymentProcessingResult.RedirectUrl);
            }

            message = paymentProcessingResult.Message;
            return paymentProcessingResult.IsSuccessful;
        }

        /// <summary>
        /// Processes the payment.
        /// </summary>
        /// <param name="orderGroup">The order group.</param>
        /// <param name="payment">The payment.</param>
        public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment)
        {
            if (HttpContext.Current == null)
            {
                return PaymentProcessingResult.CreateSuccessfulResult("ProcessPaymentNullHttpContext");
            }

            if (payment == null)
            {
                return PaymentProcessingResult.CreateUnsuccessfulResult("PaymentNotSpecified");
            }

            var orderForm = orderGroup.Forms.FirstOrDefault(f => f.Payments.Contains(payment));
            if (orderForm == null)
            {
                return PaymentProcessingResult.CreateUnsuccessfulResult("PaymentNotAssociatedOrderForm");
            }

            var requestOrder = CreateRequestOrder(orderGroup, payment);
            var message = string.Empty;
            var response = _alepayClient.RequestOrderToAlepay(requestOrder);
            if (response == null)
            {
                return PaymentProcessingResult.CreateUnsuccessfulResult(message);
            }

            var redirectUrl = response.CheckoutUrl;
            message = $"---Alepay CreatePreOrder is successful. Redirect end user to {redirectUrl}";
            return PaymentProcessingResult.CreateSuccessfulResult(message, redirectUrl);
        }

        private RequestOrder CreateRequestOrder(IOrderGroup orderGroup, IPayment payment)
        {
            var orderNumberID = _orderNumberGenerator.GenerateOrderNumber(orderGroup);

            var order = new RequestOrder();
            order.OrderCode = orderNumberID;
            order.Currency = orderGroup.Currency.CurrencyCode;
            order.Amount = payment.Amount;
            order.TotalItem = orderGroup.GetAllLineItems().Count();
            order.ReturnUrl = $"{HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority)}{_cmsPaymentPropertyService.GetPayooPaymentProcessingPage()}";
            order.CancelUrl = $"{HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority)}{_cmsPaymentPropertyService.GetCancelledPaymentUrl()}";

            var aleCheckoutParams = _cmsPaymentPropertyService.GetCheckoutParams(orderGroup);
            order.CheckoutType = aleCheckoutParams.CheckoutType;
            order.AllowDomestic = aleCheckoutParams.AllowDomestic;
            order.Installment = aleCheckoutParams.Installment;
            order.BankCode = aleCheckoutParams.BankCode;
            order.PaymentMethod = aleCheckoutParams.PaymentMethod;
            order.Month = aleCheckoutParams.Month;

            var customer = CustomerContext.Current.GetContactById(orderGroup.CustomerId);
            order.BuyerName = customer.FullName ?? $"{customer.FirstName} {customer.MiddleName} {customer.LastName}";
            order.BuyerPhone = "123456789";
            order.BuyerEmail = customer.Email;
            order.BuyerAddress = customer.PreferredShippingAddress?.Line1;
            order.BuyerCity = customer.PreferredShippingAddress?.City;
            order.BuyerCountry = customer.PreferredShippingAddress?.CountryName;

            order.OrderDescription = "This is order descritpion";
            return order;
        }
    }
}

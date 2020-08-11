using System;
using System.Linq;
using System.Web;
using EPiServer.Commerce.Order;
using EPiServer.Data;
using EPiServer.Logging.Compatibility;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Security;

namespace Foundation.Commerce.Payment.Alepay
{
    public interface IAlepayCartService
    {
        string GetDefaultCartName();
        ICart LoadDefaultCart();
        IPurchaseOrder ProcessSuccessfulTransaction(ICart cart, IPayment payment);
        string ProcessUnsuccessfulTransaction(string cancelUrl, string errorMessage);
        IPurchaseOrder MakePurchaseOrder(ICart cart, IPayment payment);
    }

    [ServiceConfiguration(typeof(IAlepayCartService), Lifecycle = ServiceInstanceScope.Transient)]
    public class AlepayCartService : IAlepayCartService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(AlepayCartService));

        private readonly IOrderRepository _orderRepository;
        private static Lazy<DatabaseMode> _databaseMode = new Lazy<DatabaseMode>(() => GetDefaultDatabaseMode());

        public AlepayCartService() : this(ServiceLocator.Current.GetInstance<IOrderRepository>())
        { }

        public AlepayCartService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public virtual string GetDefaultCartName() => Cart.DefaultName;

        public virtual ICart LoadDefaultCart() => _orderRepository.LoadCart<ICart>(PrincipalInfo.CurrentPrincipal.GetContactId(), GetDefaultCartName());

        /// <summary>
        /// Processes the successful transaction, was called when Alepay Gateway redirect back.
        /// </summary>
        /// <param name="cart"></param>
        /// <param name="payment">The order payment.</param>
        /// <returns>The url redirection after process.</returns>
        public virtual IPurchaseOrder ProcessSuccessfulTransaction(ICart cart, IPayment payment)
        {
            if (HttpContext.Current == null || cart == null)
            {
                return null;
            }
            var paymentSuccess = ProcessPayment(cart);

            if (!paymentSuccess)
            {
                _logger.Error("Can not process payment");
                return null;
            }

            // Place order
            return MakePurchaseOrder(cart, payment);
        }

        /// <summary>
        /// Processes the unsuccessful transaction
        /// </summary>
        /// <param name="cancelUrl">The cancel url.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The url redirection after process.</returns>
        public virtual string ProcessUnsuccessfulTransaction(string cancelUrl, string errorMessage)
        {
            if (HttpContext.Current == null)
            {
                return cancelUrl;
            }

            _logger.Error($"Alepay transaction failed [{errorMessage}].");
            return UriUtil.AddQueryString(cancelUrl, "message", HttpUtility.UrlEncode(errorMessage));
        }

        public virtual IPurchaseOrder MakePurchaseOrder(ICart cart, IPayment payment)
        {
            var orderReference = _orderRepository.SaveAsPurchaseOrder(cart);
            var purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);
            purchaseOrder.OrderNumber = payment.Properties[Constant.OrderCode] as string;

            if (_databaseMode.Value != DatabaseMode.ReadOnly)
            {
                // Update last order date time for CurrentContact
                UpdateLastOrderTimestampOfCurrentContact(CustomerContext.Current.CurrentContact, purchaseOrder.Created);
            }

            AddNoteToPurchaseOrder(string.Empty, $"New order placed by {PrincipalInfo.CurrentPrincipal.Identity.Name} in Public site", Guid.Empty, purchaseOrder);

            // Remove old cart
            _orderRepository.Delete(cart.OrderLink);
            purchaseOrder.OrderStatus = OrderStatus.InProgress;

            _orderRepository.Save(purchaseOrder);

            return purchaseOrder;
        }

        public virtual bool ProcessPayment(ICart cart)
        {
            try
            {
                foreach (IPayment p in cart.Forms.SelectMany(f => f.Payments).Where(p => p != null))
                {
                    PaymentStatusManager.ProcessPayment(p);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Process Payment Failed", ex);
                return false;
            }
        }

        /// <summary>
        /// Update last order time stamp which current user completed.
        /// </summary>
        /// <param name="contact">The customer contact.</param>
        /// <param name="datetime">The order time.</param>
        protected void UpdateLastOrderTimestampOfCurrentContact(CustomerContact contact, DateTime datetime)
        {
            if (contact != null)
            {
                contact.LastOrder = datetime;
                contact.SaveChanges();
            }
        }

        /// <summary>
        /// Adds the note to purchase order.
        /// </summary>
        /// <param name="title">The note title.</param>
        /// <param name="detail">The note detail.</param>
        /// <param name="customerId">The customer Id.</param>
        /// <param name="purchaseOrder">The purchase order.</param>
        protected void AddNoteToPurchaseOrder(string title, string detail, Guid customerId, IPurchaseOrder purchaseOrder)
        {
            var orderNote = purchaseOrder.CreateOrderNote();
            orderNote.Type = OrderNoteTypes.System.ToString();
            orderNote.CustomerId = customerId != Guid.Empty ? customerId : PrincipalInfo.CurrentPrincipal.GetContactId();
            orderNote.Title = !string.IsNullOrEmpty(title) ? title : detail.Substring(0, Math.Min(detail.Length, 24)) + "...";
            orderNote.Detail = detail;
            orderNote.Created = DateTime.UtcNow;
            purchaseOrder.Notes.Add(orderNote);
        }

        private static DatabaseMode GetDefaultDatabaseMode()
        {
            if (!_databaseMode.IsValueCreated)
            {
                return ServiceLocator.Current.GetInstance<IDatabaseMode>().DatabaseMode;
            }
            return _databaseMode.Value;
        }
    }
}

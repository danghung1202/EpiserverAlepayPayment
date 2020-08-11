using System;
using Mediachase.Commerce.Orders;
using Mediachase.MetaDataPlus.Configurator;

namespace Foundation.Commerce.Payment.Alepay
{
    /// <summary>
    /// Represents Payment class for Payoo.
    /// </summary>
    [Serializable]
    public class AlepayPayment : Mediachase.Commerce.Orders.Payment
    {
        private static MetaClass _metaClass;

        public AlepayPayment()
            : base(PayooPaymentMetaClass)
        {
            PaymentType = PaymentType.Other;
            ImplementationClass = GetType().AssemblyQualifiedName; // need to have assembly name in order to retrieve the correct type in ClassInfo
        }

        /// <summary>
        /// Gets the alepay payment meta class.
        /// </summary>
        /// <value>The credit card payment meta class.</value>
        public static MetaClass PayooPaymentMetaClass => _metaClass ?? (_metaClass = MetaClass.Load(OrderContext.MetaDataContext, "AlepayPayment"));

        public string TransactionCode
        {
            get { return GetString(Constant.TransactionCode); }
            set { this[Constant.TransactionCode] = value; }
        }

        public string OrderCode
        {
            get { return GetString(Constant.OrderCode); }
            set { this[Constant.OrderCode] = value; }
        }

        public string OrderAmount
        {
            get { return GetString(Constant.OrderAmount); }
            set { this[Constant.OrderAmount] = value; }
        }

        public string Currency
        {
            get { return GetString(Constant.Currency); }
            set { this[Constant.Currency] = value; }
        }

        public string CardNumber
        {
            get { return GetString(Constant.CardNumber); }
            set { this[Constant.CardNumber] = value; }
        }

        public string CardType
        {
            get { return GetString(Constant.CardType); }
            set { this[Constant.CardType] = value; }
        }

        public string TransactionStatus
        {
            get { return GetString(Constant.TransactionStatus); }
            set { this[Constant.TransactionStatus] = value; }
        }

        public string Message
        {
            get { return GetString(Constant.Message); }
            set { this[Constant.Message] = value; }
        }

        public string Installment
        {
            get { return GetString(Constant.Installment); }
            set { this[Constant.Installment] = value; }
        }

        public string Is3D
        {
            get { return GetString(Constant.Is3D); }
            set { this[Constant.Is3D] = value; }
        }

        public string Month
        {
            get { return GetString(Constant.Month); }
            set { this[Constant.Month] = value; }
        }

        public string BankCode
        {
            get { return GetString(Constant.BankCode); }
            set { this[Constant.BankCode] = value; }
        }

        public string BankName
        {
            get { return GetString(Constant.BankName); }
            set { this[Constant.BankName] = value; }
        }

        public string TransactionTime
        {
            get { return GetString(Constant.TransactionTime); }
            set { this[Constant.TransactionTime] = value; }
        }

        public string SuccessTime
        {
            get { return GetString(Constant.SuccessTime); }
            set { this[Constant.SuccessTime] = value; }
        }

        public string MerchantFee
        {
            get { return GetString(Constant.MerchantFee); }
            set { this[Constant.MerchantFee] = value; }
        }

        public string PayerFee
        {
            get { return GetString(Constant.PayerFee); }
            set { this[Constant.PayerFee] = value; }
        }
    }
}

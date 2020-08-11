namespace Foundation.Commerce.Payment.Alepay
{
    public enum CheckoutTypes
    {
        /// <summary>
        /// Normal payment with Visa/master/jcb card and installment
        /// </summary>
        CreditCardAndInstallment,
        /// <summary>
        /// Normal payment only with Visa/jcb/ master card
        /// </summary>
        CreditCard,
        /// <summary>
        /// Installment payment only
        /// </summary>
        Installment,
        /// <summary>
        /// Normal payment with ATM, IB, QRCODE, Visa/master/jcb card and installment if allowDomestic = true
        /// </summary>
        CreditCardAndInstallmentAndDomesticCard,
        /// <summary>
        /// Normal payment only with ATM, IB, QRCODE, Visa/master/jcb card if allowDomestic = true
        /// </summary>
        CreditCardAndDomesticCard
    }

    public class AlepayCheckoutParams
    {
        public virtual int CheckoutType { get; set; }
        public virtual bool Installment { get; set; }
        public virtual bool AllowDomestic { get; set; }
        public virtual int Month { get; set; }
        public virtual string BankCode { get; set; }
        public virtual string PaymentMethod { get; set; }
    }

    public class DefaultAlepayCheckoutParams : AlepayCheckoutParams
    {
        public override int CheckoutType => (int)CheckoutTypes.CreditCardAndInstallment;
        public override bool Installment => false;
        public override bool AllowDomestic => false;
    }

    public class CreditCardParams
    {
        public int CheckoutType => (int)CheckoutTypes.CreditCard;
        public bool Installment => false;
        public bool AllowDomestic => false;
    }

    public class InstallmentParams
    {
        public int CheckoutType => (int)CheckoutTypes.Installment;
        public bool Installment => true;
        public bool AllowDomestic => false;
        public int Month { get; set; }
        public string BankCode { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class DomesticCardParams
    {
        public int CheckoutType => (int)CheckoutTypes.CreditCardAndDomesticCard;
        public bool Installment => false;
        public bool AllowDomestic => true;
        public string BankCode { get; set; }
        public string PaymentMethod { get; set; }
    }
}
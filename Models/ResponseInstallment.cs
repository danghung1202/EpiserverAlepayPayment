namespace Foundation.Commerce.Payment.Alepay
{
    public class ResponseInstallment
    {
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public Payment[] PaymentMethods { get; set; }
    }

    public class Payment
    {
        public string PaymentMethod { get; set; }
        public PaymentPeriod[] Periods { get; set; }
    }

    public class PaymentPeriod
    {
        public decimal Month { get; set; }
        public decimal AmountFee { get; set; }
        public decimal AmountFinal { get; set; }
        public decimal AmountByMonth { get; set; }
        public string Currency { get; set; }
    }
}
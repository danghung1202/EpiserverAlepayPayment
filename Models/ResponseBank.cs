namespace Foundation.Commerce.Payment.Alepay
{
    public class ResponseBank
    {
        public BankInfo[] Data { get; set; }
    }
    public class BankInfo
    {
        public string BankCode { get; set; }
        public string BankFullName { get; set; }
        public string MethodCode { get; set; }
        public string PaymentMethodBankId { get; set; }
        public string PaymentMethodId { get; set; }
        public string MethodId { get; set; }
    }
}
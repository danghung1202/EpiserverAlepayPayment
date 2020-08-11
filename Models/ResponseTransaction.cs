namespace Foundation.Commerce.Payment.Alepay
{
    public class ResponseTransaction
    {
        public string TransactionCode { get; set; }
        public string OrderCode { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerPhone { get; set; }
        public string CardNumber { get; set; }
        public string BuyerName { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string Installment { get; set; }
        public string Is3D { get; set; }
        public string Month { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string Method { get; set; }
        public string TransactionTime { get; set; }
        public string SuccessTime { get; set; }
        public string BankHotline { get; set; }
    }
}
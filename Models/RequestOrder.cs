namespace Foundation.Commerce.Payment.Alepay
{
    public class RequestOrder
    {
        public string OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string OrderDescription { get; set; }
        public int TotalItem { get; set; }
        public int CheckoutType { get; set; }
        public bool AllowDomestic { get; set; }
        public string BankCode { get; set; }
        public string PaymentMethod { get; set; }
        public bool Installment { get; set; }
        public int Month { get; set; }
        public string ReturnUrl { get; set; }
        public string CancelUrl { get; set; }
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerPhone { get; set; }
        public string BuyerAddress { get; set; }
        public string BuyerCity { get; set; }
        public string BuyerCountry { get; set; }
        public string PaymentHours { get; set; }
        public string PromotionCode { get; set; }
    }
    public class ResponseOrder
    {
        public string Token { get; set; }
        public string CheckoutUrl { get; set; }
    }
}
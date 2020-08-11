namespace Foundation.Commerce.Payment.Alepay
{
    public class AlepayResponse
    {
        public string ErrorCode { get; set; }
        public string Data { get; set; }
        public string Checksum { get; set; }
        public string ErrorDescription { get; set; }
    }

    public class ResponseCallback
    {
        public string ErrorCode { get; set; }
        public string Data { get; set; }
        public string Cancel { get; set; }
    }
}

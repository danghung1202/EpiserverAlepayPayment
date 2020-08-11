using System;
using System.Collections.Generic;
using EPiServer.Logging.Compatibility;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using RestSharp;

namespace Foundation.Commerce.Payment.Alepay
{
    public interface IAlepayClient
    {
        ResponseOrder RequestOrderToAlepay(RequestOrder requestOrder);
        ResponseTransaction GetTransactionInfo(string transactionCode);
        ResponseTransaction GetTransactionInfoFromResponseCallback(string responseCallbackData);
        IEnumerable<BankInfo> GetBankDomestic(decimal orderAmount);
        IEnumerable<ResponseInstallment> GetInstallmentInfo(decimal installmentAmount, string currencyCode);
    }

    [ServiceConfiguration(typeof(IAlepayClient), Lifecycle = ServiceInstanceScope.HttpContext)]
    public class AlepayClient : IAlepayClient
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(AlepayClient));

        private readonly string _baseUrl;
        private readonly string _tokenKey;
        private readonly string _checksumKey;
        private readonly RSACrypt _rsaCrypt;

        private static class AleUri
        {
            public static string RequestOrder = "/checkout/v1/request-order";
            public static string GetTransactionInfo = "/checkout/v1/get-transaction-info";
            public static string GetBankDomestic = "/checkout/v1/get-banks-domestic";
            public static string GetInstallmentInfo = "/checkout/v1/get-installment-info";
        }

        //public AlepayClient(): this(new AlepayConfiguration()){}

        public AlepayClient()
        {
            var configuration = new AlepayConfiguration();
            _baseUrl = configuration.AlepayApiUrl;
            _tokenKey = configuration.TokenKey;
            _checksumKey = configuration.ChecksumKey;
            _rsaCrypt = new RSACrypt(configuration.EncryptKey);
        }

        public ResponseOrder RequestOrderToAlepay(RequestOrder requestOrder)
        {
            return SendRequestToAlepay<ResponseOrder>(requestOrder, $"{this._baseUrl}{AleUri.RequestOrder}");
        }

        public ResponseTransaction GetTransactionInfo(string transactionCode)
        {
            return SendRequestToAlepay<ResponseTransaction>(new { transactionCode }, $"{this._baseUrl}{AleUri.GetTransactionInfo}");
        }

        public ResponseTransaction GetTransactionInfoFromResponseCallback(string responseCallbackData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(responseCallbackData);
            var base64DecodedString = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

            var responseCallback = _rsaCrypt.Decrypt<ResponseCallback>(base64DecodedString);
            return GetTransactionInfo(responseCallback.Data);
        }

        public IEnumerable<BankInfo> GetBankDomestic(decimal orderAmount)
        {
            var responseBank = SendRequestToAlepay<ResponseBank>(new { amount = orderAmount }, $"{this._baseUrl}{AleUri.GetBankDomestic}");
            return responseBank?.Data;
        }

        public IEnumerable<ResponseInstallment> GetInstallmentInfo(decimal installmentAmount, string currencyCode)
        {
            return SendRequestToAlepay<IEnumerable<ResponseInstallment>>(new { amount = installmentAmount, currencyCode }, $"{this._baseUrl}{AleUri.GetInstallmentInfo}");
        }

        private T SendRequestToAlepay<T>(object data, string url)
        {
            try
            {
                var encryptedData = _rsaCrypt.Encrypt(data);
                var checksum = MD5Crypt.MD5Hash($"{encryptedData}{this._checksumKey}");

                var client = new RestClient(url);
                var request = new RestRequest(Method.POST);
                var requestBody = new
                {
                    data = encryptedData,
                    checksum = checksum,
                    token = this._tokenKey
                };
                request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(requestBody), ParameterType.RequestBody);
                var response = client.Execute(request);

                if (response.ErrorException != null) throw response.ErrorException;

                var alepayResponse = JsonConvert.DeserializeObject<AlepayResponse>(response.Content);
                return alepayResponse.ErrorCode == "000" ? _rsaCrypt.Decrypt<T>(alepayResponse.Data) : default(T);
            }
            catch (Exception ex)
            {
                _logger.Error("Error in SendRequestToAlepay", ex);
                return default(T);
            }
        }
    }
}

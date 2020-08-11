using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

namespace Foundation.Commerce.Payment.Alepay
{
    /// <summary>
    /// Represents Payoo configuration data.
    /// </summary>
    public class AlepayConfiguration
    {
        private PaymentMethodDto _paymentMethodDto;
        private IDictionary<string, string> _settings;

        public const string SystemName = "Alepay";

        public const string AlepayApiUrlParameter = "AlepayApiUrl";
        public const string TokenKeyParameter = "TokenKey";
        public const string EncryptKeyParameter = "EncryptKey";
        public const string ChecksumKeyParameter = "ChecksumKey";

        public string AlepayApiUrl { get; protected set; }
        public string ChecksumKey { get; protected set; }
        public string TokenKey { get; protected set; }
        public string EncryptKey { get; protected set; }

        public Guid PaymentMethodId { get; protected set; }

        /// <summary>
        /// Initializes a new instance of <see cref="AlepayConfigurationn"/>.
        /// </summary>
        public AlepayConfiguration() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="AlepayConfigurationn"/> with specific settings.
        /// </summary>
        /// <param name="settings">The specific settings.</param>
        public AlepayConfiguration(IDictionary<string, string> settings)
        {
            Initialize(settings);
        }

        /// <summary>
        /// Gets the PaymentMethodDto's parameter (setting in CommerceManager of Payoo) by name.
        /// </summary>
        /// <param name="paymentMethodDto">The payment method dto.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <returns>The parameter row.</returns>
        public static PaymentMethodDto.PaymentMethodParameterRow GetParameterByName(PaymentMethodDto paymentMethodDto, string parameterName)
        {
            var rowArray = (PaymentMethodDto.PaymentMethodParameterRow[])paymentMethodDto.PaymentMethodParameter.Select($"Parameter = '{parameterName}'");
            return rowArray.Length > 0 ? rowArray[0] : null;
        }

        protected virtual void Initialize(IDictionary<string, string> settings)
        {
            _paymentMethodDto = GetAlepayPaymentMethod();
            PaymentMethodId = GetPaymentMethodId();

            _settings = settings ?? GetSettings();
            GetParametersValues();
        }

        public static PaymentMethodDto GetAlepayPaymentMethod()
        {
            return PaymentManager.GetPaymentMethodBySystemName(SystemName, SiteContext.Current.LanguageName);
        }

        private Guid GetPaymentMethodId()
        {
            var paymentMethodRow = _paymentMethodDto.PaymentMethod.Rows[0] as PaymentMethodDto.PaymentMethodRow;
            return paymentMethodRow?.PaymentMethodId ?? Guid.Empty;
        }

        private IDictionary<string, string> GetSettings()
        {
            return _paymentMethodDto.PaymentMethod
                .FirstOrDefault()
                ?.GetPaymentMethodParameterRows()
                ?.ToDictionary(row => row.Parameter, row => row.Value);
        }

        private void GetParametersValues()
        {
            if (_settings != null)
            {
                AlepayApiUrl = GetParameterValue(AlepayApiUrlParameter);
                ChecksumKey = GetParameterValue(ChecksumKeyParameter);
                TokenKey = GetParameterValue(TokenKeyParameter);
                EncryptKey = GetParameterValue(EncryptKeyParameter);
            }
        }

        private string GetParameterValue(string parameterName)
        {
            string parameterValue;
            return _settings.TryGetValue(parameterName, out parameterValue) ? parameterValue : string.Empty;
        }

    }
}

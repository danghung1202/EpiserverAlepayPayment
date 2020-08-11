using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.Interfaces;
using System;
using System.Web.UI.WebControls;

namespace Foundation.Commerce.Payment.Alepay
{
    public partial class ConfigurePayment : System.Web.UI.UserControl, IGatewayControl
    {
        private PaymentMethodDto _paymentMethodDto;

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>The validation group.</value>
        public string ValidationGroup { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurePayment"/> class.
        /// </summary>
        public ConfigurePayment()
        {
            ValidationGroup = string.Empty;
            _paymentMethodDto = null;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            BindData();
        }

        /// <summary>
        /// Loads the PaymentMethodDto object.
        /// </summary>
        /// <param name="dto">The PaymentMethodDto object.</param>
        public void LoadObject(object dto)
        {
            _paymentMethodDto = dto as PaymentMethodDto;
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="dto">The dto.</param>
        public void SaveChanges(object dto)
        {
            if (!Visible)
            {
                return;
            }

            _paymentMethodDto = dto as PaymentMethodDto;
            if (_paymentMethodDto != null && _paymentMethodDto.PaymentMethodParameter != null)
            {
                var paymentMethodId = Guid.Empty;
                if (_paymentMethodDto.PaymentMethod.Count > 0)
                {
                    paymentMethodId = _paymentMethodDto.PaymentMethod[0].PaymentMethodId;
                }

                UpdateOrCreateParameter(AlepayConfiguration.AlepayApiUrlParameter, AlepayApiUrl, paymentMethodId);
                UpdateOrCreateParameter(AlepayConfiguration.ChecksumKeyParameter, ChecksumKey, paymentMethodId);
                UpdateOrCreateParameter(AlepayConfiguration.TokenKeyParameter, TokenKey, paymentMethodId);
                UpdateOrCreateParameter(AlepayConfiguration.EncryptKeyParameter, EncryptKey, paymentMethodId);
            }
        }

        /// <summary>
        /// Binds the data.
        /// </summary>
        private void BindData()
        {
            if (_paymentMethodDto != null && _paymentMethodDto.PaymentMethodParameter != null)
            {
                BindParamterData(AlepayConfiguration.AlepayApiUrlParameter, AlepayApiUrl);
                BindParamterData(AlepayConfiguration.ChecksumKeyParameter, ChecksumKey);
                BindParamterData(AlepayConfiguration.TokenKeyParameter, TokenKey);
                BindParamterData(AlepayConfiguration.EncryptKeyParameter, EncryptKey);
            }
            else
            {
                Visible = false;
            }
        }

        private void UpdateOrCreateParameter(string parameterName, TextBox parameterControl, Guid paymentMethodId)
        {
            var parameter = GetParameterByName(parameterName);
            if (parameter != null)
            {
                parameter.Value = parameterControl.Text;
            }
            else
            {
                var row = _paymentMethodDto.PaymentMethodParameter.NewPaymentMethodParameterRow();
                row.PaymentMethodId = paymentMethodId;
                row.Parameter = parameterName;
                row.Value = parameterControl.Text;
                _paymentMethodDto.PaymentMethodParameter.Rows.Add(row);
            }
        }

        private void BindParamterData(string parameterName, TextBox parameterControl)
        {
            var parameterByName = GetParameterByName(parameterName);
            if (parameterByName != null)
            {
                parameterControl.Text = parameterByName.Value;
            }
        }

        private PaymentMethodDto.PaymentMethodParameterRow GetParameterByName(string name)
        {
            return AlepayConfiguration.GetParameterByName(_paymentMethodDto, name);
        }
    }
}
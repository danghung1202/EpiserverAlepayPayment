# EpiserverAlepayPayment

## How installment?

After installment Alepay payment, following these steps to setting payment method:

### For Editor

0. In Commerce Manager, you must import the AlepayPayment MetaClass from the file `AlepayPaymentMetaClass.xml`

Go CM > Administration > Order System > Meta Classes > Import MetaClass -> Drag the `AlepayPaymentMetaClass.xml` to import

1. In Commerce Manager, create the Alepay payment method

> The `System Keyword` must have value of `Alepay`

Then input the settings for Alepay payment gateway in tab Parameters.

For example:

Alepay Api Url: `https://alepay-sandbox.nganluong.vn`

Token Key: `AXDhPuFRdFWQ2q2DgjA6czwyV5SmS7`

Encrypt Key: `MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCVD14j4+4MCvVfsUFN1+724SXpwNYuVu0izw9awJJJypsTyt6WSZzRb3HhElcHd8IaAlDlFDoQkyXHYaUH43bLeb/VnNWX6kna8VCXEx/AX7Lvc0bCjKr8fB66zzhAlL3DJRwysFeGtpd0+W44XFHxuglruUVPRZ8klcznzXYVfwIDAQAB`

Checksum Key: `MHVxYcCHOulC5ST1X8dfOvjdBNPuDK`

> You can get these configurations by register the account at `https://alepay-sandbox.nganluong.vn/` and active the merchant account



2. In Episerver CMS, create the page with page type of `AlepayPaymentPage`
   
3. In Episerver CMS, the StartPage must have three `ContentReference` properties as below:
   
   * `CheckoutPage` -> point to your Checkout page
   * `AlepayPaymentPage` -> point to the Alepay Payment Page
   * `OrderConfirmationPage` -> point to your order confirmation page

> These property's names can be override via the `ICmsPaymentPropertyService`

### For Developer

1. User choose payment method and place order, the site will redirect to AlePay payment gateway

When process payment, the result will contain the redirect url. In the method which placed order, you should redirect to this url. All payment parameters which user chose such as payment type (Credit, Installment or ATM), Bank Code, Payment Method (VISA, JCB or MASTERCARD) and Periods Month (3, 6, 9, 12 or 24) should store in `Cart` extend property has name of `AlepayCheckoutParams`

> This property's names can be override via the `ICmsPaymentPropertyService`

For each payment type, there are three classes which you have to create `CreditCardParams`, `InstallmentParams`, `DomesticCardParams`

For example:

User choose Credit Card to payment:

```csharp
    cart.Properties["AlepayCheckoutParams"] = JsonConvert.SerializeObject(new CreditCardParams());
```
User choose ATM Bank to payment:
```csharp
    cart.Properties["AlepayCheckoutParams"] = JsonConvert.SerializeObject(new InstallmentParams()
    {
        Month = 3,
        BankCode = "SACOMBANK",
        PaymentMethod = "VISA"
    });
```

User choose Installment to payment:
```csharp
    cart.Properties["AlepayCheckoutParams"] = JsonConvert.SerializeObject(new DomesticCardParams()
    {
        BankCode = "SACOMBANK",
        PaymentMethod = "VISA"
    });
```

> Since the value of `AlepayCheckoutParams` property don't need to save to DB, you don't need add this property into `ShoppingCart` MetaClass on Initialize module.


Then on place order when you process payment

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> PlaceOrder(CheckoutPage currentPage, CheckoutViewModel checkoutViewModel){
    ...
    ...

    //Call method to process payment
    var processPayments = cart.ProcessPayments(_paymentProcessor, _orderGroupCalculator);
    var paymentResult = processPayments.FirstOrDefault();

    if (paymentResult == null || !paymentResult.IsSuccessful) return false;
    //Redirect to alepay payment gateway
    if (!string.IsNullOrWhiteSpace(paymentResult.RedirectUrl))
    {
        return Redirect(paymentRedirectUrl);
    }

    ...
    ...

}

```

1. User input the payment information on Alepay and submit. if all information is correct, Alepay will redirect to `AlepayPaymentPage` in your site

2. The `AlepayPaymentPage` will get transaction information and convert cart to purchase order. All functionalities was implemented by `IAlepayCartService` which you can override.


## Alepay APIs

All apis are provided via interface `IAlepayClient`

```csharp

    public interface IAlepayClient
    {
        ResponseOrder RequestOrderToAlepay(RequestOrder requestOrder);
        ResponseTransaction GetTransactionInfo(string transactionCode);
        IEnumerable<BankInfo> GetBankDomestic(decimal orderAmount);
        IEnumerable<ResponseInstallment> GetInstallmentInfo(decimal installmentAmount, string currencyCode);
    }

```

1. To get all bank domestic, using the method `GetBankDomestic`

Input: the Order Total Amount

Output

```javascript

[
  {
    "BankCode": "NCB",
    "BankFullName": "Ngân Hàng Quốc Dân",
    "MethodCode": "QRCODE",
    "PaymentMethodBankId": "1111",
    "PaymentMethodId": "2426",
    "MethodId": "240"
  },
  {
    "BankCode": "NVB",
    "BankFullName": "Ngân Hàng Quốc Dân",
    "MethodCode": "ATM_ON",
    "PaymentMethodBankId": "1111",
    "PaymentMethodId": "1030",
    "MethodId": "35"
  },
  {
    "BankCode": "PGB",
    "BankFullName": "PGBank - Xăng dầu Petrolimex",
    "MethodCode": "ATM_ON",
    "PaymentMethodBankId": "1119",
    "PaymentMethodId": "1051",
    "MethodId": "35"
  }
  ...
]

```

1. To get all installment info, using the method `GetInstallmentInfo`

Input: the Order Total Amount and currency (VND, USD...)

Output

```javascript
[
  {
    "BankCode": "SACOMBANK",
    "BankName": "NH TMCP Sài Gòn Thương Tín",
    "PaymentMethods": [
      {
        "PaymentMethod": "VISA",
        "Periods": [
          {
            "Month": 24,
            "AmountFee": 1180000,
            "AmountFinal": 29180000,
            "AmountByMonth": 1215834,
            "Currency": "VND"
          },
          {
            "Month": 12,
            "AmountFee": 841000,
            "AmountFinal": 28841000,
            "AmountByMonth": 2403417,
            "Currency": "VND"
          },
          ...
        ]
      },
      {
        "PaymentMethod": "JCB",
        "Periods": [
          {
            "Month": 3,
            "AmountFee": 732000,
            "AmountFinal": 28732000,
            "AmountByMonth": 9577334,
            "Currency": "VND"
          },
          ...
        ]
      },
      ...
    ]
  }
  ...
]

```
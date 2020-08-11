using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace Foundation.Commerce.Payment.Alepay
{
    [ContentType(GUID = "2F1C9A42-DE63-41C1-9D85-E6A3205F8B37",
        DisplayName = "Alepay Payment Page",
        Description = "Alepay Payment Process Page.",
        GroupName = "Payment",
        Order = 100)]
    public class AlepayPaymentPage : PageData
    {
    }
}

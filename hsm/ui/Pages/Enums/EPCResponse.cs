using System.ComponentModel;

namespace OCC.HSM.UI.Pages.Enums
{
    public enum EPCResponse
    {
        [Description("The postcode lookup didn't find any certificates in that area. You can proceed without postcode information, though it may lead to less applicable suggestions.")]
        NoCertificateFound = 0,
        [Description("Energy performance certificates are currently not available, you can proceed without postcode information")]
        EPCServiceCurrentlyNotAvailable = 1,
        [Description("This isn't a valid postcode. Check it and enter it again.")]
        InvalidPostCode = 2,
        [Description("Please enter a valid postcode.")]
        PostCodeIsNullOrWhiteSpace = 3,
        [Description("Valid postcode.")]
        CertificateFound = 4,
        [Description("Please select an address.")]
        AddressIsNullOrWhiteSpace = 5,
    }
}

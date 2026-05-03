using System;
using Xunit;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;

namespace RP.Enterprise.Subsystem.ProductLauncher.Tests.Landing.Logic.Product
{
    public class ManageProductRPDocumentManagement_IsProductUrlValid_Tests
    {
        // Use reflection so we don't depend on InternalsVisibleTo
        private static bool InvokeIsProductUrlValid(string productUrl, string domain)
        {
            var method = typeof(ManageProductRPDocumentManagement)
                .GetMethod("IsProductUrlValid", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            Assert.NotNull(method);
            return (bool)method.Invoke(null, new object[] { productUrl, domain });
        }

        [Fact]
        public void IsProductUrlValid_RejectsBlueBookPlaceholderDomain_AndAcceptsRealHost()
        {
            // The placeholder text from BlueBook that caused the production exception
            string placeholder = "This product is not yet implemented. The product access page will be available when your implementation is complete. Thank you for choosing RealPage.";
            string template = "https://{{domain}}-rpdd.realpage.com";
            string badUrl = template.Replace("{{domain}}", placeholder);

            // Bad input must be rejected (would have caused 'Invalid URI: The hostname could not be parsed.')
            Assert.False(InvokeIsProductUrlValid(badUrl, placeholder),
                "Placeholder domain must be rejected to avoid unparseable URI.");

            // Empty/null inputs must be rejected
            Assert.False(InvokeIsProductUrlValid(null, "acme"));
            Assert.False(InvokeIsProductUrlValid("https://acme-rpdd.realpage.com", null));
            Assert.False(InvokeIsProductUrlValid("", ""));

            // Domain containing whitespace must be rejected
            Assert.False(InvokeIsProductUrlValid("https://bad domain-rpdd.realpage.com", "bad domain"));

            // A real, well-formed URL must be accepted
            string goodUrl = "https://acme-rpdd.realpage.com";
            Assert.True(InvokeIsProductUrlValid(goodUrl, "acme"),
                "Valid host should be accepted.");
        }
    }
}

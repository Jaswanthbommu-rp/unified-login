using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Collections.Generic;
using System.Data;

namespace GreenBook.Tests.LandingApiTests.Products
{
    public class Products : TestController
    {
        private string payload;
        JsonController jsonManager = new JsonController();
        TestUtilities reusable;
        private readonly ITestOutputHelper XunitTestOutPut;
        private string realPageId = "";
        private DataTable expectedContactMechanism = new DataTable();

        public Products(ITestOutputHelper _xUnitTestOutput)
        {
            this.XunitTestOutPut = _xUnitTestOutput;
        }

        // Profiles=/api/Profiles/

        [Fact, Trait("", "Happy Path")]
        public void GetProductTypes()
        {
            // Set up the API URL
            EndPointUrl = HostUrl + Properties["ProductTypes"];

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            // Extract API's JSON Response
            //List<ProductType> productFamilyResponse = JsonConvert.DeserializeObject<List<ProductType>>(ResponseString);
            ObjectListOutput<ProductType, IErrorData> productTypeResponse = JsonConvert.DeserializeObject<ObjectListOutput<ProductType, IErrorData>>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == response.StatusCode");

            foreach (var product in productTypeResponse.list)
            {
                Assert.NotNull(product.ProductTypeGuid);
                Assert.True(product.ProductTypeId > 0);
                Assert.NotNull(product.Name);
                Assert.NotNull(product.Description);

                if (product.ParentProductTypeName != null)
                {
                    Assert.True(product.ParentProductTypeId > 0);
                }
            }
        }


        // [Fact, Trait("", "Happy Path")]
        public void GetProductTypesUsingDB()
        {
            // Set up the API URL
            EndPointUrl = HostUrl + Properties["ProductTypes"];

            // Extract Expected JSON Response
            ProductType expectedProductType = JsonConvert.DeserializeObject<ProductType>(reusable.DoGetProductType());
            XunitTestOutPut.WriteLine("Expected JSON Response:\n" + JsonConvert.SerializeObject(expectedProductType));

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            // Extract API's JSON Response
            IList<ProductType> productFamilyResponse = JsonConvert.DeserializeObject<IList<ProductType>>(ResponseString);
            ObjectListOutput<ProductType, IErrorData> productTypeResponse = JsonConvert.DeserializeObject<ObjectListOutput<ProductType, IErrorData>>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == response.StatusCode");

            for (int countProductFamily = 0; countProductFamily < productFamilyResponse.Count; countProductFamily++)
            {
                Assert.NotNull(productFamilyResponse[0].ProductTypeGuid);
                Assert.NotNull(productFamilyResponse[0].ProductTypeId);
                Assert.True(productFamilyResponse[0].ProductTypeId == expectedProductType.ProductTypeId,
                    "productFamilyResponse[0].ProductTypeId == expectedProductFamily.ProductTypeId");
                Assert.NotNull(productFamilyResponse[0].Name);
                Assert.True(productFamilyResponse[0].Name == expectedProductType.Name,
                    "productFamilyResponse[0].Name == expectedProductFamily.Name");
                Assert.NotNull(productFamilyResponse[0].Description);
                Assert.True(productFamilyResponse[0].Description == expectedProductType.Description,
                    "productFamilyResponse[0].Description == expectedProductFamily.Description");
            }
        }
    }
}

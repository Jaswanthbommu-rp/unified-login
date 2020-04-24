using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.WebHook;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
    [ExcludeFromCodeCoverage]
    public class WebHookTests
    {
        private readonly Mock<IRepository> _mockRepository = new Mock<IRepository>();
        private readonly Mock<IUnitOfWork> _mockUnitofWork = new Mock<IUnitOfWork>();

        private static Guid _RealPageId = new Guid("C802694D-5553-4527-8616-3C0F434AE62D");
        private static Guid _adminRealPageId = new Guid("C802694D-1111-2222-3333-3C0F434AE62D");
        private static string _CompanyName = "Test Company";
        private static DateTime _CreateDate = DateTime.MaxValue.ToUniversalTime();
        private static int _PartyId = 54321;
        private static long _BooksMasterId = 12345;
        private static long _BooksCompanyMasterId = 12345;
        private static int _organizationTypeId = 6;
        private static string _organizationTypeName = "Multifamily";

        private string _mockJsonDataPayload = "{\r\n\t\t\"link\": \"/customercompany?filter[customerCompanyId]={customerCompanyId}&filter[deletedAt]=not:null\",\r\n\t\t\"payload\": {\r\n    \t\t\"customerCompanyId\": 15862,\r\n    \t\t\"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n    \t\t\"replacementCustomerCompanyId\"  : 9999\r\n\t\t}\r\n\t}";
        private string _mockJsonData = "";

        private readonly string _mockJsonDataSignature = "094353bb6ebc40d323a5f9dd7269dc6c39afae770638dc44fccb0f66027749fa";

        private List<OrganizationType> _organizationTypeList;
        private Organization _organization = null;

        public WebHookTests()
        { 
            _organization = new Organization()
            {
                RealPageId = _RealPageId,
                CreateDate = _CreateDate,
                Name = _CompanyName,
                PartyId = _PartyId,
                BooksMasterId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = _organizationTypeId,
                organizationType = new OrganizationType()
                {
                    OrganizationTypeId = _organizationTypeId
                }
            };

            _mockJsonData = "{\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\n\t\"topic\": \"books.customercompany.deleted\",\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\n\t\"payload\": " + _mockJsonDataPayload + "\n}\n";
            
            _mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(_mockUnitofWork.Object);

            _mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            _mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DataImportMappingUpdate, It.IsAny<object>()))
                .Returns(new RepositoryResponse {Id = 1, ErrorMessage = ""});


            _organizationTypeList = new List<OrganizationType>()
            {
                new OrganizationType()
                {
                    OrganizationTypeId = 6,
                    Name = "Multifamily",
                    CreateDate = new DateTime()
                },
                new OrganizationType()
                {
                    OrganizationTypeId = 14,
                    Name = "Vendor",
                    CreateDate = new DateTime()
                },
                new OrganizationType()
                {
                    OrganizationTypeId = 7,
                    Name = "Other",
                    CreateDate = new DateTime()
                }
            };

            // THIS RESULT IS CACHED SO WE CANT REALLY TEST IT HAVING MULTIPLE RESULTS!
            _mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);
        }

        [Fact]
        public void Update_BooksMasterId_Success()
        {
            //Arrange
            WebHookController webHookController = new WebHookController(
                _mockRepository.Object
            )
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books")
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJsonData);
            webHookController.Request.Headers.Add("signature", _mockJsonDataSignature);

            webHookController.Configuration = new HttpConfiguration();

            ThinEvent<JToken> thinEvent = new ThinEvent<JToken>()
            {
                Id = "601e13a6-7360-ceda-bf0c-41c62fa694c7",
                Topic = "books.customercompany.deleted",
                CreatedAt = DateTime.Now,
                Payload = JsonConvert.DeserializeObject<JToken>(_mockJsonDataPayload)
            };
            

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            //string message = response.Content.ReadAsStringAsync().Result;


            string expectedValue = "{\"Message\":\"Duplicate master ids\"}";

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
            //Assert.True(expectedValue == message);
        }
    }
}

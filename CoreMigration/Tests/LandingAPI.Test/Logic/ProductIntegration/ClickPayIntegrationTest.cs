using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Test.Logic.ProductIntegration
{
	[ExcludeFromCodeCoverage]
	public class ClickPayIntegrationTest
	{
		ProductEnum productType = ProductEnum.ClickPay;
		GbProductMap gbProductMap = new GbProductMap { Name = "Payments", BooksProductCode = "Payments", ProductId = 48 };

		static readonly string CompanyInstanceSourceId = ClickPayTestData.CompanyInstanceSourceId;
		static readonly string SubjectUserLoginName = ClickPayTestData.SubjectUserLoginName;
		static readonly string UpdateInputJson = ClickPayTestData.UpdateInputJson;

		private Mock<IDataCollector> dataCollector;
		private Mock<IManagePersona> managePersona;
		private Mock<IProductInternalSettingRepository> productInternalSettingRepository;

		private CustomerCompanyMap customerCompanyMap;
		readonly List<ProductInternalSetting> productInternalSettingList;

		long editorPersonaId = 33;
		long subjectPersonaId = 0;
		Persona persona = new Persona { PersonaId = 33 };

		private UserDetails editorUserDetails = new UserDetails
		{
			BooksMasterId = 1,
			BooksCustomerMasterId = 1,
			PersonaId = 33
		};

		private UserDetails subjectUserDetails = new UserDetails
		{
			BooksMasterId = 1,
			BooksCustomerMasterId = 1,
			PersonaId = 34,
			LoginName = SubjectUserLoginName,
			ProductUserName = SubjectUserLoginName
		};

		DefaultUserClaim userClaims = new DefaultUserClaim()
		{
			LoginName = "MocTest",
			CorrelationId = Guid.NewGuid(),
			OrganizationName = "MocTest",
			OrganizationPartyId = 1,
			OrganizationRealPageGuid = Guid.NewGuid(),
			OrganizationMasterId = 1,
			UserRealPageGuid = Guid.NewGuid(),
			PersonaId = 33,
		};

		public ClickPayIntegrationTest()
		{
			productInternalSettingList = ClickPayTestData.Get_DEV_CPAY_ProductSettings();

			customerCompanyMap = new CustomerCompanyMap { CompanyInstanceSourceId = CompanyInstanceSourceId };

			dataCollector = new Mock<IDataCollector>();
			managePersona = new Mock<IManagePersona>();
			productInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

			dataCollector.Setup(m => m.GetUserDetailsByPersona(editorPersonaId, (int)productType)).Returns(editorUserDetails);
			dataCollector.Setup(m => m.GetBlueBookProductMap((int)productType)).Returns(gbProductMap);
			dataCollector.Setup(m => m.GetProductCompanyMap(gbProductMap.BooksProductCode, editorUserDetails.BooksMasterId, userClaims, "Primary")).Returns(customerCompanyMap);
			managePersona.Setup(m => m.GetPersona(editorPersonaId)).Returns(persona);
			productInternalSettingRepository.Setup(m => m.GetProductInternalSettings((int)productType)).Returns(productInternalSettingList);
		}

		[Fact(Skip = "Integration test")]
		public void GetRoles_Test()
		{
			var productLogic =
				ManageProductFactory.GetProductLogic(productType, editorPersonaId, subjectPersonaId, userClaims,
					dataCollector.Object, managePersona.Object, productInternalSettingRepository.Object);
			var result = productLogic.GetProductRoles(null);

			Assert.True(result.Records.Count > 2);
		}

		[Fact(Skip = "Integration test")]
		public void GetProductOrganizations_owner_Test()
		{
			var productLogic =
				ManageProductFactory.GetProductLogic(productType, editorPersonaId, subjectPersonaId, userClaims,
					dataCollector.Object, managePersona.Object, productInternalSettingRepository.Object);
			var result = productLogic.GetProductOrganizations("0", "owner");

			Assert.True(result.Records.Count > 2);
		}

		[Fact(Skip = "Integration test")]
		public void GetProductOrganizations_site_Test()
		{
			var productLogic =
				ManageProductFactory.GetProductLogic(productType, editorPersonaId, subjectPersonaId, userClaims,
					dataCollector.Object, managePersona.Object, productInternalSettingRepository.Object);
			var result = productLogic.GetProductOrganizations("0", "site");

			Assert.True(result.Records.Count > 2);
		}

		[Fact(Skip = "Integration test")]
		public void GetUser_Test()
		{
			var result = GetUser();
			Assert.Equal(result.LoginName, SubjectUserLoginName);
		}

		private IntegrationProductUser GetUser()
		{
			// Subject User Data
			subjectPersonaId = 34;
			dataCollector.Setup(m => m.GetUserDetailsByPersona(subjectPersonaId, (int)productType)).Returns(subjectUserDetails);

			var productLogic =
				ManageProductFactory.GetProductLogic(productType, editorPersonaId, subjectPersonaId, userClaims,
					dataCollector.Object, managePersona.Object, productInternalSettingRepository.Object);
			return productLogic.GetProductUser();
		}

		[Fact(Skip = "Integration test")]
		public void UpdateUser_Test()
		{
            List<AdditionalParameters> additionalParameters;
            // Subject User Data
            subjectPersonaId = 34;
			dataCollector.Setup(m => m.GetUserDetailsByPersona(subjectPersonaId, (int)productType)).Returns(subjectUserDetails);
			dataCollector.Setup(m => m.UpdateProductSettingProductStatus(subjectPersonaId, "ProductStatus",
				(int)productType, (int)ProductBatchStatusType.Success));

			dataCollector.Setup(m => m.UpdateSamlUserAttribute(subjectPersonaId, (int)productType, SamlAttributeEnum.productUsername,
				subjectUserDetails.Email));

			var productUser = GetUser();

			if (productUser == null)
				throw new Exception("Unable to get user details for product, check productUserLoginName is correct.");

			subjectUserDetails.Email = productUser.Email;
			subjectUserDetails.FirstName = productUser.FirstName;
			subjectUserDetails.LastName = productUser.LastName;
			subjectUserDetails.LoginName = productUser.LoginName;
			subjectUserDetails.PhoneNumber = productUser.Phone;
			subjectUserDetails.ProductUserId = productUser.UserId;

			var productPropertiesRoles =
				GetProductPropertiesRoles<ProductUserRolePropertiesGroups>(UpdateInputJson);

			var productLogic =
				ManageProductFactory.GetProductLogic(productType, editorPersonaId, subjectPersonaId, userClaims,
					dataCollector.Object, managePersona.Object, productInternalSettingRepository.Object);
			var result = productLogic.CreateUpdateProductUser(productPropertiesRoles, out additionalParameters);

			Assert.Equal(result, string.Empty);
		}

		[Fact(Skip = "Integration test")]
		public void CreateUser_Test()
		{
            List<AdditionalParameters> additionalParameters;
            // Subject User Data
            subjectPersonaId = 34;
			dataCollector.Setup(m => m.GetUserDetailsByPersona(subjectPersonaId, (int)productType)).Returns(subjectUserDetails);

			dataCollector.Setup(m => m.UpdateProductSettingProductStatus(subjectPersonaId, "ProductStatus",
				(int)productType, (int)ProductBatchStatusType.Success));

			dataCollector.Setup(m => m.UpdateSamlUserAttribute(subjectPersonaId, (int)productType, SamlAttributeEnum.productUsername,
				subjectUserDetails.Email));

			var randomFirstName = DateTime.Now.Ticks.ToString();

			subjectUserDetails.Email = $"E{randomFirstName}@test.com";
			subjectUserDetails.FirstName = $"F{randomFirstName}@test.com";
			subjectUserDetails.LastName = $"L{randomFirstName}@test.com";
			subjectUserDetails.LoginName = $"E{randomFirstName}@test.com";
			subjectUserDetails.PhoneNumber = "";
			subjectUserDetails.ProductUserId = "";
			subjectUserDetails.ProductUserName = "";

			var productPropertiesRoles =
				GetProductPropertiesRoles<ProductUserRolePropertiesGroups>(UpdateInputJson);

			var productLogic =
				ManageProductFactory.GetProductLogic(productType, editorPersonaId, subjectPersonaId, userClaims,
					dataCollector.Object, managePersona.Object, productInternalSettingRepository.Object);
			var result = productLogic.CreateUpdateProductUser(productPropertiesRoles, out additionalParameters);

			Assert.Equal(result, string.Empty);
		}

		private T GetProductPropertiesRoles<T>(string productUserInputJson)
		{
			if (string.IsNullOrEmpty(productUserInputJson))
				return default(T); //throw new Exception("productUserInputJson is null or empty");

			try
			{
				return JsonConvert.DeserializeObject<T>(productUserInputJson.Trim());
			}
			catch (Exception ex)
			{
				// if the parser fails return an empty object so the product call can catch the error
				return default(T);
			}
		}
	}
}

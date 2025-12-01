using Moq;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.LandingAPI.Test.Logic;
using UnifiedLogin.LandingAPI;
using UnifiedLogin.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace UnifiedLogin.LandingAPI.Test.ControllerTest
{
	/// <summary>
	/// Postal Address xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class PostalAddressTests
	{
		#region Controller Unit Tests
		[Fact]
		public void LinkPostalAddress_InvalidRealPageId_ExceptionThrown()
		{
			//Arrange
			Guid realPageId = new Guid();
			ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
			{
				ContactMechanismUsageTypeId = 201,
				ParentContactMechanismUsageTypeId = 200,
				Name = "Primary"
			};

			PartyContactMechanism partyContactMechanism = new PartyContactMechanism()
			{
				FromDate = DateTime.UtcNow,
				ThruDate = DateTime.MaxValue.ToUniversalTime()
			};

			ContactMechanismBoundary contactMechanismBoundary = new ContactMechanismBoundary()
			{
				FromDate = DateTime.UtcNow,
				ThruDate = DateTime.MaxValue.ToUniversalTime()
			};

			StreetAddress streetAddress = new StreetAddress()
			{
				StreetAddress1 = "2201 Lakeside Blvd"
			};

			GeographicBoundaryType geographicBoundaryType = new GeographicBoundaryType()
			{
				TypeName = "City"
			};

			GeographicBoundary geographicBoundary = new GeographicBoundary()
			{
				Name = "Richardson",
				GeographicBoundaryType = geographicBoundaryType
			};

			IList<GeographicBoundary> geographicBoundaryList = new List<GeographicBoundary>();
			geographicBoundaryList.Add(geographicBoundary);

			LinkPostalAddress linkPostalAddress = new LinkPostalAddress()
			{
				PartyContactMechanism = partyContactMechanism,
				ContactMechanismUsageType = contactMechanismUsageType,
				ContactMechanismBoundary = contactMechanismBoundary,
				StreetAddress = streetAddress,
				GeographicBoundary = geographicBoundaryList
			};

			PostalAddressController postalAddressController = new PostalAddressController();

			//Act
			Exception exception = Record.Exception(() => postalAddressController.LinkPostalAddress(realPageId, linkPostalAddress));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void LinkPostalAddress_InvalidLinkPostalAddress_ExceptionThrown()
		{
			//Arrange
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			LinkPostalAddress linkPostalAddress = new LinkPostalAddress();
			PostalAddressController postalAddressController = new PostalAddressController();

			//Act
			Exception exception = Record.Exception(() => postalAddressController.LinkPostalAddress(realPageId, linkPostalAddress));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void LinkPostalAddress_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("LinkPostalAddress" == baseTest.VerifyRouteToAction(
				HttpMethod.Post,
				"http://localhost/api/persons/13E71DE5-BAFA-469D-9F7A-E12DB3961BA9/postaladdress"
				)
			);
		}

		[Fact]
		public void ListPostalAddressForPerson_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("ListPostalAddressForPerson" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/persons/13E71DE5-BAFA-469D-9F7A-E12DB3961BA9/postaladdress"
				)
			);
		}

		[Fact]
		public void ListPostalAddressForPerson_InvalidRealPageId_ExceptionThrown()
		{
			//Arrange
			Guid realPageId = new Guid();
			PostalAddressController postalAddressController = new PostalAddressController();

			//Act
			Exception exception = Record.Exception(() => postalAddressController.ListPostalAddressForPerson(realPageId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void ListPostalAddressForPerson_MockRepository_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			string ContactMechanismUsageTypeName = "";
			ObjectListOutput<PostalAddress, IErrorData> postalAddressListOutput = new ObjectListOutput<PostalAddress, IErrorData>();
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			Type type = typeof(PostalAddress);
			List<PostalAddress> expectedPostalAddressList = new List<PostalAddress>();
			ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
			{
				ContactMechanismUsageTypeId = 201,
				ParentContactMechanismUsageTypeId = 200,
				Name = "Primary"
			};

			PostalAddress postalAddress = new PostalAddress()
			{
				PartyContactMechanismId = 1,
				ContactMechanismId = 1,
				AddressString = "Richardson",
				AddressType = "City",
				contactMechanismUsageType = contactMechanismUsageType
			};
			expectedPostalAddressList.Add(postalAddress);

			postalAddress = new PostalAddress()
			{
				PartyContactMechanismId = 1,
				ContactMechanismId = 1,
				AddressString = "Dallas",
				AddressType = "County",
				contactMechanismUsageType = contactMechanismUsageType
			};
			expectedPostalAddressList.Add(postalAddress);

			var mockRepository = new Mock<IPostalAddressRepository>();
			mockRepository
				.Setup(m => m.ListPostalAddressForPerson(realPageId, ContactMechanismUsageTypeName))
				.Returns(() => expectedPostalAddressList);

			PostalAddressController controller = new PostalAddressController(mockRepository.Object);
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpClient();

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			HttpResponseMessage response = controller.ListPostalAddressForPerson(realPageId);
			postalAddressListOutput = response.Content.ReadAsAsync<ObjectListOutput<PostalAddress, IErrorData>>().Result;

			//Assert
			Assert.True(response.IsSuccessStatusCode);
			Assert.Equal(response.RequestMessage.Method.ToString(), "GET");
			Assert.Equal(postalAddressListOutput.list.Count, 2);
			Assert.True(postalAddressListOutput.list.Count == expectedPostalAddressList.Count);
		}
		#endregion
	}
}

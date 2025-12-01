using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.LandingAPI;
using UnifiedLogin.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
	/// <summary>
	/// Profile xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ProfileTest
	{
		#region Controller Unit Tests
		[Fact]
		public void GetProfile_InvalidRealPageId_ExceptionThrown()
		{
			//Arrange
			Guid realPageId = new Guid();
			string roleTypeFrom = "User Role";
			string roleTypeTo = "Organization Role";
			string relationshipType = "User Relationship";
			ProfileController profileController = new ProfileController();

            //Act
            Exception exception = Record.Exception(() => profileController.GetProfileDetail(realPageId, roleTypeFrom, roleTypeTo, relationshipType));

            //Assert
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void GetProfile_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpClient Config = new HttpClient();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("GetProfileDetail" == baseTest.VerifyRouteToAction(
                HttpMethod.Get,
                "http://localhost/api/profiles/13E71DE5-BAFA-469D-9F7A-E12DB3961BA9/Organizations"
                )
            );
        }

        [Fact]
        public void UpdateProfile_InvalidRealPageId_ExceptionThrown()
        {
            //Arrange
            Guid realPageId = new Guid();

            ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
            {
                ContactMechanismUsageTypeId = 201,
                ParentContactMechanismUsageTypeId = 200,
                Name = "Phone"
            };

            IList<TelecommunicationNumber> telecomunicationNumberList = new List<TelecommunicationNumber>();
            TelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber()
            {
                PartyContactMechanismId = 10003,
                ContactMechanismId = 1003,
                CountryCode = "01",
                AreaCode = "972",
                PhoneNumber = "8204000",
                IsDefault = true,
                contactMechanismUsageType = contactMechanismUsageType
            };

            telecomunicationNumberList.Add(telecommunicationNumber);

            PartyRole partyRole = new PartyRole()
            {
                PartyRoleId = 1003,
                PartyId = 19,
                RoleTypeId = 402
            };

			Profile profile = new Profile()
			{
				PartyId = 1,
				Title = "Property Manager",
				FirstName = "John",
				MiddleName = "R",
				LastName = "Doe",
				Suffix = "Mr",
				PreferredContactMethodId = 1,
				TelecommunicationNumber = telecomunicationNumberList,
				PartyRole = partyRole
			};
            ProfileController profileController = new ProfileController();

            //Act
            Exception exception = Record.Exception(() => profileController.UpdateProfile(realPageId, profile));

            //Assert
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void UpdateProfile_InvalidPerson_ExceptionThrown()
        {
            //Arrange
            Guid realPageId = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202");
            Profile profile = new Profile();
            ProfileController profileController = new ProfileController();

            //Act
            Exception exception = Record.Exception(() => profileController.UpdateProfile(realPageId, profile));

            //Assert
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void UpdateProfile_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpClient Config = new HttpClient();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("UpdateProfile" == baseTest.VerifyRouteToAction(
                HttpMethod.Put,
                "http://localhost/api/profiles/13E71DE5-BAFA-469D-9F7A-E12DB3961BA9"
                )
            );
        }

        [Fact]
        public void GetProfileDetail_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpClient Config = new HttpClient();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("GetProfileDetail" == baseTest.VerifyRouteToAction(
                HttpMethod.Get,
                "http://localhost/api/profiles/details"
                )
            );
        }
        #endregion
    }
}

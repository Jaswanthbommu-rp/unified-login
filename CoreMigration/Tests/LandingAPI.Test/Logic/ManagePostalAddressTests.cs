using Moq;
using UnifiedLogin.BusinessLogic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnifiedLogin.BusinessLogic.Logic;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
	/// <summary>
	/// Postal Address Unit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManagePostalAddressTests
	{
		[Fact]
		public void ListPostalAddress_InvalidrealPageId_ExceptionThrown()
		{
			//Arrange
			string ContactMechanismUsageTypeName = "";
			IManagePostalAddress managePostalAddress = new ManagePostalAddress();

			//Act
			Guid realPageId = new Guid();

			//Assert
			Assert.Throws<Exception>(() => managePostalAddress.ListPostalAddressForPerson(realPageId, ContactMechanismUsageTypeName));
		}

		[Fact]
		public void ListPostalAddressForPerson_RealPageIdNotExistinDatabase_ReturnEmptyObject()
		{
			//Arrange
			string ContactMechanismUsageTypeName = "";

			Type type = typeof(PostalAddress);
			IPostalAddress postalAddress = new PostalAddress();
			IList<IPostalAddress> expectedPostalAddressList = new List<IPostalAddress>();
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			var mockObject = new Mock<IPostalAddressRepository>();
			mockObject
				.Setup(m => m.ListPostalAddressForPerson(realPageId, ContactMechanismUsageTypeName))
				.Returns(() => new List<PostalAddress> { new PostalAddress() { } });

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			expectedPostalAddressList.Add(postalAddress);
			IManagePostalAddress managePostalAddress = new ManagePostalAddress(mockObject.Object);
			IList<PostalAddress> postalAddressList = managePostalAddress.ListPostalAddressForPerson(realPageId, ContactMechanismUsageTypeName);

			//Assert
			Assert.True(
				postalAddressList.Count == 1
				&& expectedPostalAddressList.Count == 1
				&& postalAddressList.All(
					o => expectedPostalAddressList.Any(
						w => w.AddressString == o.AddressString
						&&
						w.AddressType == o.AddressType
						&&
						w.ContactMechanismId == o.ContactMechanismId
						&&
						w.contactMechanismUsageType == o.contactMechanismUsageType
						&&
						w.PartyContactMechanismId == o.PartyContactMechanismId
					)
				) == true
				&& NumberOfProperties == 6
			);
		}
	}
}

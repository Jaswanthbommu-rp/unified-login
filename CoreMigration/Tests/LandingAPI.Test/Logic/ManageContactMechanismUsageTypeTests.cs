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
	/// Contact Mechanism UsageType xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManageContactMechanismUsageTypeTests
	{
		[Fact]
		public void ListContactMechanismUsageType_Mock_ReturnValidContactMechanismUsageTypeList()
		{
			//Arrange
			string ContactMechanismUsageTypeName = "";
			Type type = typeof(ContactMechanismUsageType);

			List<ContactMechanismUsageType> expectedContactMechanismUsageTypeList = new List<ContactMechanismUsageType>()
			{
				new ContactMechanismUsageType()
				{
					ContactMechanismUsageTypeId = 101,
					ParentContactMechanismUsageTypeId = 100,
					Name = "Primary"
				},
				new ContactMechanismUsageType()
				{
					ContactMechanismUsageTypeId = 102,
					ParentContactMechanismUsageTypeId = 100,
					Name = "Secondary"
				}
			};

			var mockObject = new Mock<IContactMechanismUsageTypeRepository>();
			mockObject
				.Setup(m => m.ListContactMechanismUsageType(ContactMechanismUsageTypeName))
				.Returns(() => expectedContactMechanismUsageTypeList);

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			IManageContactMechanismUsageType manageContactMechanismUsageType = new ManageContactMechanismUsageType(mockObject.Object);
			IList<ContactMechanismUsageType> contactMechanismUsageTypeList = manageContactMechanismUsageType.ListContactMechanismUsageType(ContactMechanismUsageTypeName);

			//Assert
			Assert.True(
				contactMechanismUsageTypeList.Count == expectedContactMechanismUsageTypeList.Count
				&& contactMechanismUsageTypeList.All (
					o => expectedContactMechanismUsageTypeList.Any (
						w => w.ContactMechanismUsageTypeId == o.ContactMechanismUsageTypeId
						&&
						w.ParentContactMechanismUsageTypeId == o.ParentContactMechanismUsageTypeId
						&&
						w.Name == o.Name
					)
				) == true
				&& NumberOfProperties == 3
			);
		}
	}
}
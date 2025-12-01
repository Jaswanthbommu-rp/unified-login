using Moq;
using UnifiedLogin.BusinessLogic;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
	[ExcludeFromCodeCoverage]
	/// <summary>
	/// StatusType xUnit tests
	/// </summary>
	public class ManageStatusTypeTests
	{
		#region Private Variables
		IManageStatusType _manageStatusType;
		Mock<IStatusTypeRepository> _mockStatusTypeRepository = new Mock<IStatusTypeRepository>();
		#endregion

		[Fact]
		public void ListStatusType_InvalidCategoryTypeName_ExceptionThrown()
		{
			//Arrange
			string categoryTypeName = string.Empty;
			string categoryName = "User Status";

			_manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

			//Act
			Exception exception = Record.Exception(() => _manageStatusType.GetStatusType(categoryTypeName, categoryName));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void ListStatusType_InvalidCategoryName_ExceptionThrown()
		{
			//Arrange
			string categoryTypeName = "Status";
			string categoryName = string.Empty;

			_manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

			//Act
			Exception exception = Record.Exception(() => _manageStatusType.GetStatusType(categoryTypeName, categoryName));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void ListStatusType_Mock_ReturnValidStatusTypeList()
		{
			//Arrange
			string categoryTypeName = "Status";
			string categoryName = "User Status";
			Type type = typeof(StatusType);
			IList<StatusType> expectedStatusTypeList = new List<StatusType>()
			{
				new StatusType()
				{
					StatusTypeId = 1,
					Name = "Active"
				},
				new StatusType()
				{
					StatusTypeId = 24,
					Name = "Disabled"
				},
				new StatusType()
				{
					StatusTypeId = 23,
					Name = "Expired"
				},
				new StatusType()
				{
					StatusTypeId = 12,
					Name = "ForceResetPassword"
				},
				new StatusType()
				{
					StatusTypeId = 3,
					Name = "Locked"
				},
				new StatusType()
				{
					StatusTypeId = 2,
					Name = "Pending"
				}
			};

			_mockStatusTypeRepository = new Mock<IStatusTypeRepository>();
			_mockStatusTypeRepository
				.Setup(m => m.GetStatusType(categoryTypeName, categoryName))
				.Returns(() => expectedStatusTypeList);

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			IManageStatusType manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);
			IList<StatusType> statusTypeList = manageStatusType.GetStatusType(categoryTypeName, categoryName);

			//Assert
			Assert.True(
				statusTypeList.Count == expectedStatusTypeList.Count
				&& statusTypeList.All(
					o => expectedStatusTypeList.Any(
						w => w.StatusTypeId == o.StatusTypeId
						&&
						w.Name == o.Name
					)
				) == true
				&& NumberOfProperties == 2
			);
		}
	}
}

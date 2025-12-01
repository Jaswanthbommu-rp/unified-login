using Moq;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
	[ExcludeFromCodeCoverage]
	public class ManageCustomFieldsTests : ManageProductBaseTests
	{
		#region Private Variables
		IManageCustomFields _manageCustomFields;
		Mock<ICustomFieldsRepository> _mockCustomFieldsRepository = new Mock<ICustomFieldsRepository>();
		#endregion

		public ManageCustomFieldsTests() : base((int)ProductEnum.UnifiedPlatform)
		{
		}

		
		[Fact]
		public void AddUpdateFieldValue_InvalidCreatedBy_ExceptionThrown()
		{
			//Arrange
			_manageCustomFields = new ManageCustomFields(_userUserClaim);
			IList<CustomFieldValue> customFieldValueList = new List<CustomFieldValue>()
			{
				new CustomFieldValue()
				{
					FieldValueId = 1,
					UserLoginPersonaId = 1,
					Value = "12345",
					FieldId = 15,
					OrganizationId = 350,
					Enabled = true,
					Name = "Employee ID",
					Description = null,
					FieldTypeId = 1,
					FieldTypeName = "Alphanumeric",
					Required = false,
					ReadOnly = false,
					DefaultValue = null,
					SyncField = null,
					Sequence = 1,
					HelpText = null,
					MinCharLength = 1,
					MaxCharLength = 10
				}
			};

			//Act
			string customFieldsValuesJson = JsonConvert.SerializeObject(customFieldValueList);
			long createdBy = 0;

			//Assert
			Assert.Throws<Exception>(() => _manageCustomFields.AddUpdateFieldValue(customFieldsValuesJson, createdBy));
		}

		[Fact]
		public void AddUpdateFieldValue_InvalidCustomFieldsValuesJson_ExceptionThrown()
		{
			//Arrange
			_manageCustomFields = new ManageCustomFields(_userUserClaim);

			//Act
			string customFieldsValuesJson = string.Empty;
			long createdBy = 1;

			//Assert
			Assert.Throws<ArgumentNullException>(() => _manageCustomFields.AddUpdateFieldValue(customFieldsValuesJson, createdBy));
		}

		[Fact]
		public void AddUpdateFieldValue_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			_manageCustomFields = new ManageCustomFields(_userUserClaim);
			IList<CustomFieldValue> customFieldValueList = new List<CustomFieldValue>()
			{
				new CustomFieldValue()
				{
					FieldValueId = 1,
					UserLoginPersonaId = 1,
					Value = "12345",
					FieldId = 15,
					OrganizationId = 350,
					Enabled = true,
					Name = "Employee ID",
					Description = null,
					FieldTypeId = 1,
					FieldTypeName = "Alphanumeric",
					Required = false,
					ReadOnly = false,
					DefaultValue = null,
					SyncField = null,
					Sequence = 1,
					HelpText = null,
					MinCharLength = 1,
					MaxCharLength = 10
				}
			};

			string customFieldsValuesJson = JsonConvert.SerializeObject(customFieldValueList);
			long createdBy = 1;

			//Act
			_mockCustomFieldsRepository = new Mock<ICustomFieldsRepository>();
			_mockCustomFieldsRepository
				.Setup(m => m.AddUpdateFieldValue(customFieldsValuesJson, createdBy))
				.Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

			_manageCustomFields = new ManageCustomFields(_mockCustomFieldsRepository.Object, _userUserClaim);
			IRepositoryResponse repositoryResponse = _manageCustomFields.AddUpdateFieldValue(customFieldsValuesJson, createdBy);

			//Assert
			Assert.True(
				repositoryResponse.Id == 1
				&& repositoryResponse.ErrorMessage == ""
			);
		}

		
		[Fact]
		public void GetCustomField_InvalidbookMasterId_ExceptionThrown()
		{
			//Arrange
			_manageCustomFields = new ManageCustomFields(_userUserClaim);

			//Act
			RequestParameter datafilter = new RequestParameter();
			datafilter.Pages.ResultsPerPage = 0;
			IDictionary<object, object> globals = new Dictionary<object, object>();
			globals.Add(BaseType.RequestParameter, datafilter);
			long booksCustomerMasterId = 0;
			long OrganizationId = 0;
			//Assert
			Assert.Throws<Exception>(() => _manageCustomFields.GetCustomField(globals, OrganizationId));
		}

		[Fact]
		public void GetCustomField_Mock_ReturnValidCustomFields()
		{
			//Arrange
			RequestParameter datafilter = new RequestParameter();
			datafilter.Pages.ResultsPerPage = 0;
			datafilter.Pages.StartRow = 1;
			IDictionary<object, object> globals = new Dictionary<object, object>();
			globals.Add(BaseType.RequestParameter, datafilter);
			long booksCustomerMasterId = 5094;
			long OrganizationId = 350;
			int bookMasterTypeId = (int)BookMasterType.CustomerMasterId;
			Type type = typeof(IList<CustomField>);

			IList<CustomField> expectedCustomFieldList = new List<CustomField>()
			{
				new CustomField()
				{
					FieldId = 15,
					OrganizationId = 350,
					Enabled = true,
					Name = "Employee ID",
					Description = null,
					FieldTypeId = 1,
					FieldTypeName = "Alphanumeric",
					Required = false,
					ReadOnly = false,
					DefaultValue = null,
					SyncField = null,
					Sequence = 1,
					HelpText = null,
					MinCharLength = 1,
					MaxCharLength = 10
				}
			};

			_mockCustomFieldsRepository = new Mock<ICustomFieldsRepository>();
			_mockCustomFieldsRepository
				.Setup(m => m.GetCustomField(OrganizationId, datafilter))
				.Returns(() => expectedCustomFieldList);

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			_manageCustomFields = new ManageCustomFields(_mockCustomFieldsRepository.Object, _userUserClaim);
			IList<CustomField> customFieldsList = _manageCustomFields.GetCustomField(globals, OrganizationId);

			//Assert
			Assert.True(
				customFieldsList.Count == expectedCustomFieldList.Count
				&&
				customFieldsList.SequenceEqual(expectedCustomFieldList)
				&&
				NumberOfProperties == 1
			);
		}

		[Fact]
		public void GetCustomFieldsValues_InvalidorganizationPartyId_ExceptionThrown()
		{
			//Arrange
			_manageCustomFields = new ManageCustomFields(_userUserClaim);

			//Act
			long organizationPartyId = 0;
			long? userLoginPersonaId = null;
			bool enabled = true;

			//Assert
			Assert.Throws<Exception>(() => _manageCustomFields.GetCustomFieldsValues(organizationPartyId, userLoginPersonaId, enabled));
		}

		[Fact]
		public void GetCustomFieldsValues_Mock_ReturnValidCustomFields()
		{
			//Arrange
			long organizationPartyId = 350;
			long? userLoginPersonaId = 1;
			bool enabled = true;
			Type type = typeof(IList<CustomField>);

			IList<CustomFieldValue> expectedCustomFieldValueList = new List<CustomFieldValue>()
			{
				new CustomFieldValue()
				{
					FieldValueId = 1,
					UserLoginPersonaId = 1,
					Value = "12345",
					FieldId = 15,
					OrganizationId = 350,
					Enabled = true,
					Name = "Employee ID",
					Description = null,
					FieldTypeId = 1,
					FieldTypeName = "Alphanumeric",
					Required = false,
					ReadOnly = false,
					DefaultValue = null,
					SyncField = null,
					Sequence = 1,
					HelpText = null,
					MinCharLength = 1,
					MaxCharLength = 10
				}
			};

			_mockCustomFieldsRepository = new Mock<ICustomFieldsRepository>();
			_mockCustomFieldsRepository
				.Setup(m => m.GetCustomFieldsValues(organizationPartyId, userLoginPersonaId, enabled))
				.Returns(() => expectedCustomFieldValueList);

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			_manageCustomFields = new ManageCustomFields(_mockCustomFieldsRepository.Object, _userUserClaim);
			IList<CustomFieldValue> customFieldValueList = _manageCustomFields.GetCustomFieldsValues(organizationPartyId, userLoginPersonaId, enabled);

			//Assert
			Assert.True(
				customFieldValueList.Count == expectedCustomFieldValueList.Count
				&&
				customFieldValueList.SequenceEqual(expectedCustomFieldValueList)
				&&
				NumberOfProperties == 1
			);
		}
	}
}

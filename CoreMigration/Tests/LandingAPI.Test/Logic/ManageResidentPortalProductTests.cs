using Moq;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Product.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using UnifiedLogin.SharedObjects.Saml;
using UnifiedLogin.LandingAPI.Test.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;
using IC = UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
	/// <summary>
	/// Product xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManageResidentPortalProductTests : ManageProductBaseTests
	{
		#region Private Variables
		private int _blueBookId;
		private List<Community> _PropertyList = new List<Community>();
		private List<ILevel> _RoleList = new List<ILevel>();
		private List<IMessagingGroups> _messageGroupsList = new List<IMessagingGroups>();
		private Notifications _notifications = new Notifications();
		private ListResponse _listResponse = new ListResponse();
		private ResidentPortalUser _residentPortalUser = new ResidentPortalUser();
		private ResidentPortalUser _residentPortalEditorUser = new ResidentPortalUser();
        private DefaultUserClaim _userClaims;

        private string testHostname = "http://localhost";
        private string _mtApiEndPoint = "http://localhost";
		private string _appId = "d8f43b85";
		private string _appKey = "50aa7342baf824716f87e6999cf4b472";
		#endregion

		#region Constructor
		public ManageResidentPortalProductTests() : base((int)ProductEnum.ResidentPortal)
        {
			_blueBookId = 236;

			_editorSamlAttributes = new List<SamlAttributes>();
			_editorSamlAttributes.Add(new SamlAttributes() { Name = "UserId", Value = "1234567" });
			_editorSamlAttributes.Add(new SamlAttributes() { Name = "PRODUCTUSERNAME", Value = "bob12" });

			_userSamlAttributes = new List<SamlAttributes>();
			_userSamlAttributes.Add(new SamlAttributes() { Name = "UserId", Value = "5432" });
			_userSamlAttributes.Add(new SamlAttributes() { Name = "PRODUCTUSERNAME", Value = "larry33" });

			_productSettingType.Add(new ProductSettingType() { ProductSettingTypeId = 1, Name = "ProductStatus" });

			_userProductSettings.Add(new ProductSettingList() { ProductId = 1, Name = "IsFavorite", Value = "1", ProductSettingId = 1234 });
			_userProductSettings.Add(new ProductSettingList() { ProductId = 1, Name = "AllProperties", Value = "1", ProductSettingId = 1234 });

			_electronicAddressList = new List<IC.ElectronicAddress>();
			_electronicAddressList.Add(new IC.ElectronicAddress() { AddressType = "Email", AddressString = "test" });

			_productInternalSettings.Add(new IC.ProductInternalSetting() { Name = "ApiEndPoint", Value = testHostname });
            _productInternalSettings.Add(new IC.ProductInternalSetting() { Name = "MTAPIENDPOINT", Value = _mtApiEndPoint });
			_productInternalSettings.Add(new IC.ProductInternalSetting() { Name = "APPID", Value = _appId });
			_productInternalSettings.Add(new IC.ProductInternalSetting() { Name = "APPKEY", Value = _appKey });
			_repositoryResponseProductStatus.ErrorMessage = "";

            _userClaims = new DefaultUserClaim()
            {
                //UserId = 1,
                LoginName = "MocTest",
                CorrelationId = System.Guid.NewGuid(),
                OrganizationName = "MocTest",
                OrganizationPartyId = 1,
                OrganizationRealPageGuid = System.Guid.NewGuid(),
                OrganizationMasterId = 1,
                UserRealPageGuid = Guid.NewGuid(),
                Rights = new List<string>()
            };

            mockRepository
				.Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
				.Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);
        }
		#endregion

		#region XUnit tests
		[Fact]
		public void GetNotificationSettings_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
			Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
			Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
			Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
			Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();
			Mock<IManageProductResidentPortal> mockManageProductResidentPortal = new Mock<IManageProductResidentPortal>();

			Notifications expectedNotifications = new Notifications();

			IList<CustomerCompanyMap> companyMapList = new List<CustomerCompanyMap>()
			{
				new CustomerCompanyMap()
				{
					CompanyInstanceSourceId = _blueBookId.ToString(),
					Source = BlueBookProductConstants.ResidentPortal
				}
			};

			_editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

			mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.Is<string>(l => l == "AB"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
				 ))
				 .Returns(companyMapList);

			mockProductInternalSettingRepository
				.Setup(m => m.GetProductInternalSettings(
					It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
				))
				.Returns(_productInternalSettings);

			mockSamlRepository
				.Setup(m => m.GetProductSamlDetails(
					It.Is<long>(l => l == 4)
					, It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
				 ))
				 .Returns(_editorSamlAttributes);

			mockSamlRepository
				.Setup(m => m.GetProductSamlDetails(
					It.Is<long>(l => l == 5)
					, It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
				 ))
				 .Returns(_userSamlAttributes);

			mockManagePersona
				.Setup(m => m.GetPersona(
					It.Is<long>(l => l == 4)
				 ))
				 .Returns(_editorPersona);

			mockManagePersona
				.Setup(m => m.GetPersona(
					It.Is<long>(l => l == 5)
				 ))
				 .Returns(_userPersona);

			mockProductRepository
				.Setup(m => m.GetProductSettingsByPersona(
					It.IsAny<long>()
				))
				.Returns(_userProductSettings);

			mockProductRepository
			  .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
              ))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.ResidentPortal));

            _residentPortalUser.Notifications = expectedNotifications;

			//Act
			IManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(
				editorRealPageId: _editorRealPageId,
				residentPortalEditorUser: _residentPortalEditorUser,
				residentPortalUser: _residentPortalUser,
				samlRepository: mockSamlRepository.Object,
				managePersona: mockManagePersona.Object,
				manageBlueBook: mockManageBlueBook.Object,
				productRepository: mockProductRepository.Object,
				productInternalSettingRepository: mockProductInternalSettingRepository.Object,
				managePerson: null,
				manageUserLogin: null,
				managePartyRelationship: null,
				manageElectronicAddress: null,
                userClaim: _userClaims,
                messageHandler: mockHttpMessageHandler.Object,
				repository: mockRepository.Object);

			//Assert
			_notifications = manageProductResidentPortal.GetNotificationSettings(_editorPersonaId, _userPersonaId);
			Assert.True(_notifications.amenitiesViaEmail == expectedNotifications.amenitiesViaEmail);
			Assert.True(_notifications.managerFdiViaEmail == expectedNotifications.managerFdiViaEmail);
			Assert.True(_notifications.managerMrViaEmail == expectedNotifications.managerMrViaEmail);
		}

		[Fact]
		public void ListLevels_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
			Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
			Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
			Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
			Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();
			Mock<IManageProductResidentPortal> mockManageProductResidentPortal = new Mock<IManageProductResidentPortal>();
			Mock<RolePropertyList> mockRolePropertyList = new Mock<RolePropertyList>();

			List<ILevel> expectedLevelList = new List<ILevel>()
			{
				new Level()
				{
					Id = "STAFFSTANDARD",
					Name = "Staff Standard",
					IsAssigned = false,
                    IsDisabled = false
				},
				new Level()
				{
					Id = "STAFFADMIN",
					Name = "Staff Admin",
					IsAssigned = false,
                    IsDisabled = false
                },
				new Level()
				{
					Id = "STAFFLIMITED",
					Name = "Staff Limited",
					IsAssigned = false,
                    IsDisabled = false
                },
				new Level()
				{
					Id = "ENTERPRISESTANDARD",
					Name = "Enterprise Standard",
					IsAssigned = false,
                    IsDisabled = false
                },
				new Level()
				{
					Id = "ENTERPRISEADMIN",
					Name = "Enterprise Admin",
					IsAssigned = false,
                    IsDisabled = false
                }
			};

			IList<CustomerCompanyMap> companyMapList = new List<CustomerCompanyMap>()
			{
				new CustomerCompanyMap()
				{
					CompanyInstanceSourceId = _blueBookId.ToString(),
					Source = BlueBookProductConstants.ResidentPortal
				}
			};

			_editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

			mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.Is<string>(l => l == "AB"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
				 ))
				 .Returns(companyMapList);

			mockProductInternalSettingRepository
				.Setup(m => m.GetProductInternalSettings(
					It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
				))
				.Returns(_productInternalSettings);

			mockSamlRepository
				.Setup(m => m.GetProductSamlDetails(
					It.Is<long>(l => l == 4)
					, It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
				 ))
				 .Returns(_editorSamlAttributes);

			mockSamlRepository
				.Setup(m => m.GetProductSamlDetails(
					It.Is<long>(l => l == 5)
					, It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
				 ))
				 .Returns(_userSamlAttributes);

			mockManagePersona
				.Setup(m => m.GetPersona(
					It.Is<long>(l => l == 4)
				 ))
				 .Returns(_editorPersona);

			mockManagePersona
				.Setup(m => m.GetPersona(
					It.Is<long>(l => l == 5)
				 ))
				 .Returns(_userPersona);

			mockProductRepository
				.Setup(m => m.GetProductSettingsByPersona(
					It.IsAny<long>()
				))
				.Returns(_userProductSettings);

			mockProductRepository
			  .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
              ))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.ResidentPortal));

            Dictionary<string, string> rolesDictionary = new Dictionary<string, string>();
			rolesDictionary.Add("ENTERPRISE_ADMIN", "Enterprise Admin");
			rolesDictionary.Add("ENTERPRISE_STANDARD", "Enterprise Standard");
			rolesDictionary.Add("STAFF_ADMIN", "Staff Admin");
			rolesDictionary.Add("STAFF_STANDARD", "Staff Standard");
			rolesDictionary.Add("STAFF_LIMITED", "Staff Limited");

			_residentPortalEditorUser.canCreateRoles = rolesDictionary;

			//Act
			IManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(
				editorRealPageId: _editorRealPageId,
				residentPortalEditorUser: _residentPortalEditorUser,
				residentPortalUser: _residentPortalUser,
				samlRepository: mockSamlRepository.Object,
				managePersona: mockManagePersona.Object,
				manageBlueBook: mockManageBlueBook.Object,
				productRepository: mockProductRepository.Object,
				productInternalSettingRepository: mockProductInternalSettingRepository.Object,
				managePerson: null,
				manageUserLogin: null,
				managePartyRelationship: null,
				manageElectronicAddress: null,
                userClaim: _userClaims,
                messageHandler: mockHttpMessageHandler.Object,
                repository: mockRepository.Object);

            //Assert
            _RoleList = manageProductResidentPortal.ListLevels(0, 0);
			List<ILevel> compareResult = _RoleList.Where(item => expectedLevelList.Select(eItem => eItem.Id).Contains(item.Id)).ToList();
			Assert.True(_RoleList.Count == expectedLevelList.Count);
			Assert.True(compareResult.Count == expectedLevelList.Count);
		}

		[Fact]
		public void ListMessageGroups_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
			Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
			Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
			Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
			Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();
			//Mock<IManageProductResidentPortal> mockManageProductResidentPortal = new Mock<IManageProductResidentPortal>();

			List<IMessagingGroups> expectedMessageGroupsList = new List<IMessagingGroups>()
			{
				new MessagingGroups()
				{
					Id = "MANAGEMENT",
					Name = "Management",
					IsAssigned = false
				},
				new MessagingGroups()
				{
					Id = "RESIDENT_SERVICES",
					Name = "Resident Services",
					IsAssigned = false
				},
				new MessagingGroups()
				{
					Id = "FRONT_DESK",
					Name = "Front Desk",
					IsAssigned = false
				},
				new MessagingGroups()
				{
					Id = "MAINTENANCE",
					Name = "Maintenance",
					IsAssigned = false
				},
				new MessagingGroups()
				{
					Id = "LEASING",
					Name = "Leasing",
					IsAssigned = false
				}
			};

			IList<CustomerCompanyMap> companyMapList = new List<CustomerCompanyMap>()
			{
				new CustomerCompanyMap()
				{
					CompanyInstanceSourceId = _blueBookId.ToString(),
					Source = BlueBookProductConstants.ResidentPortal
				}
			};

			_editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

			mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.Is<string>(l => l == "AB"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
				 ))
				 .Returns(companyMapList);

			mockProductInternalSettingRepository
				.Setup(m => m.GetProductInternalSettings(
					It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
				))
				.Returns(_productInternalSettings);

			mockSamlRepository
				.Setup(m => m.GetProductSamlDetails(
					It.Is<long>(l => l == 4)
					, It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
				 ))
				 .Returns(_editorSamlAttributes);

			mockSamlRepository
				.Setup(m => m.GetProductSamlDetails(
					It.Is<long>(l => l == 5)
					, It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
				 ))
				 .Returns(_userSamlAttributes);

			mockManagePersona
				.Setup(m => m.GetPersona(
					It.Is<long>(l => l == 4)
				 ))
				 .Returns(_editorPersona);

			mockManagePersona
				.Setup(m => m.GetPersona(
					It.Is<long>(l => l == 5)
				 ))
				 .Returns(_userPersona);

			mockProductRepository
				.Setup(m => m.GetProductSettingsByPersona(
					It.IsAny<long>()
				))
				.Returns(_userProductSettings);

			mockProductRepository
			  .Setup(m => m.GetBooksMasterProductDetail(
				  It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
			  ))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.ResidentPortal));

            _residentPortalUser.MessagingGroups = expectedMessageGroupsList;

		//Act
		IManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(
				editorRealPageId: _editorRealPageId,
				residentPortalEditorUser: _residentPortalEditorUser,
				residentPortalUser: _residentPortalUser,
				samlRepository: mockSamlRepository.Object,
				managePersona: mockManagePersona.Object,
				manageBlueBook: mockManageBlueBook.Object,
				productRepository: mockProductRepository.Object,
				productInternalSettingRepository: mockProductInternalSettingRepository.Object,
				managePerson: null,
				manageUserLogin: null,
				managePartyRelationship: null,
				manageElectronicAddress: null,
                userClaim: _userClaims,
                messageHandler: mockHttpMessageHandler.Object,
                repository: mockRepository.Object);

            //Assert
            _messageGroupsList = manageProductResidentPortal.ListMessageGroups(_editorPersonaId, _userPersonaId);
			List<IMessagingGroups> compareResult = _messageGroupsList.Where(item => expectedMessageGroupsList.Select(eItem => eItem.Id).Contains(item.Id)).ToList();
			Assert.True(_messageGroupsList.Count == expectedMessageGroupsList.Count);
			Assert.True(compareResult.Count == expectedMessageGroupsList.Count);
		}

		//[Fact]
		public void ListProperties_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
			Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
			Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
			Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
			Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();

			List<ProductProperty> expectedProductPropertyList = new List<ProductProperty>()
			{
				new ProductProperty()
				{
					ID = "4305",
					Name = "Lakeline Crossing",
					IsAssigned = false,
                    disableSelection = false
				},
				new ProductProperty()
				{
					ID = "305",
					Name = "The Fountains at Memorial City",
					IsAssigned = false,
                    disableSelection = false
                },
				new ProductProperty()
				{
					ID = "379",
					Name = "7 Riverway",
					IsAssigned = false,
                    disableSelection = false
                },
				new ProductProperty()
				{
					ID = "779",
					Name = "Instrata at The Ashton Uptown",
					IsAssigned = false,
                    disableSelection = false
                },
				new ProductProperty()
				{
					ID = "1146",
					Name = "The Berkeley Apartment Homes",
					IsAssigned = false,
                    disableSelection = false
                },
				new ProductProperty()
				{
					ID = "3744",
					Name = "The Carter",
					IsAssigned = false,
                    disableSelection = false
                },
				new ProductProperty()
				{
					ID = "4264",
					Name = "2555 North Clark",
					IsAssigned = false,
                    disableSelection = false
                },
				new ProductProperty()
				{
					ID = "4288",
					Name = "Canyon Springs at Bull Creek",
					IsAssigned = false,
                    disableSelection = false
                }
			};

			IList<CustomerCompanyMap> companyMapList = new List<CustomerCompanyMap>()
			{
				new CustomerCompanyMap()
				{
					CompanyInstanceSourceId = _blueBookId.ToString(),
					Source = BlueBookProductConstants.ResidentPortal
				}
			};

			_editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

			mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.Is<string>(l => l == "AB"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
				 ))
				 .Returns(companyMapList);

			IList<PropertyInstance> propertyInstanceList = new List<PropertyInstance>()
			{
				new PropertyInstance()
				{
					PropertyInstanceSourceId = "4305",
					PropertyName = "Lakeline Crossing",
					Address = new InstanceAddress()
					{
						State = "TX"
					}
				},
				new PropertyInstance()
				{
					PropertyInstanceSourceId = "305",
					PropertyName = "The Fountains at Memorial City",
					Address = new InstanceAddress()
					{
						State = "TX"
					}
				},
				new PropertyInstance()
				{
					PropertyInstanceSourceId = "379",
					PropertyName = "7 Riverway",
					Address = new InstanceAddress()
					{
						State = "TX"
					}
				},
				new PropertyInstance()
				{
					PropertyInstanceSourceId = "779",
					PropertyName = "Instrata at The Ashton Uptown",
					Address = new InstanceAddress()
					{
						State = "TX"
					}
				},
				new PropertyInstance()
				{
					PropertyInstanceSourceId = "1146",
					PropertyName = "The Berkeley Apartment Homes",
					Address = new InstanceAddress()
					{
						State = "TX"
					}
				},
				new PropertyInstance()
				{
					PropertyInstanceSourceId = "3744",
					PropertyName = "The Carter",
					Address = new InstanceAddress()
					{
						State = "TX"
					}
				},
				new PropertyInstance()
				{
					PropertyInstanceSourceId = "4264",
					PropertyName = "2555 North Clark",
					Address = new InstanceAddress()
					{
						State = "IL"
					}
				},
				new PropertyInstance()
				{
					PropertyInstanceSourceId = "4288",
					PropertyName = "Canyon Springs at Bull Creek",
					Address = new InstanceAddress()
					{
						State = "TX"
					}
				}
			};

			mockProductInternalSettingRepository
				.Setup(m => m.GetProductInternalSettings(
					It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
				))
				.Returns(_productInternalSettings);

			mockSamlRepository
				.Setup(m => m.GetProductSamlDetails(
					It.Is<long>(l => l == 4)
					, It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
				 ))
				 .Returns(_editorSamlAttributes);

			mockSamlRepository
				.Setup(m => m.GetProductSamlDetails(
					It.Is<long>(l => l == 5)
					, It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
				 ))
				 .Returns(_userSamlAttributes);

			mockManagePersona
				.Setup(m => m.GetPersona(
					It.Is<long>(l => l == 4)
				 ))
				 .Returns(_editorPersona);

			mockManagePersona
				.Setup(m => m.GetPersona(
					It.Is<long>(l => l == 5)
				 ))
				 .Returns(_userPersona);

			mockProductRepository
				.Setup(m => m.GetProductSettingsByPersona(
					It.IsAny<long>()
				))
				.Returns(_userProductSettings);

			//Act
			IManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(
				editorRealPageId: _editorRealPageId,
				companyInstanceId: 853322,
				samlRepository: mockSamlRepository.Object,
				managePersona: mockManagePersona.Object,
				manageBlueBook: mockManageBlueBook.Object,
				productRepository: mockProductRepository.Object,
				productInternalSettingRepository: mockProductInternalSettingRepository.Object,
				managePerson: null,
				manageUserLogin: null,
				managePartyRelationship: null,
                userClaim: _userClaims,
                messageHandler: mockHttpMessageHandler.Object,
                repository: mockRepository.Object);

            //Assert
            _listResponse = manageProductResidentPortal.ListProperties(_editorPersonaId, _userPersonaId, null);
			IList<ProductProperty> productPropertyList = _listResponse.Records.Cast<ProductProperty>().ToList();
			List<ProductProperty> compareResult = productPropertyList.Where(item => expectedProductPropertyList.Select(eItem => eItem.ID).Contains(item.ID)).ToList();
			Assert.True(_listResponse.Records.Count == expectedProductPropertyList.Count);
			Assert.True(compareResult.Count == expectedProductPropertyList.Count);
		}
        #endregion

        #region Migration tests
        [Fact]
        public void GetMigrationUsers_GivenEditorIdAndDataFilter_ShouldReturnListOfResidentPortalUsers()
        {
            //Arrange
            Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
            Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
            Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
            Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();      

            IList<CustomerCompanyMap> companyMapList = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CompanyInstanceSourceId = _blueBookId.ToString(),
                    Source = BlueBookProductConstants.ResidentPortal
                }
            };

            _editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.Is<string>(l => l == "AB"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                 ))
                 .Returns(companyMapList);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, 17))))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                 ))
                 .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                 ))
                 .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                 ))
                 .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                 ))
                 .Returns(_userPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

			mockProductRepository
			  .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
              ))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.ResidentPortal));

            var filter = "NonMigrated";
            var dataFilter = new RequestParameter()
            {
                Pages = new PageRequest()
                {
                    StartRow = 1,
                    ResultsPerPage = 1000
                },
                FilterBy = new Dictionary<string, string>() {
                    { "filter" , "NonMigrated" }
                }
            };
            var totalRecords = 5;
            var url = $"{_mtApiEndPoint}/{_blueBookId}/users?filter={filter}&app_id={_appId}&app_key={_appKey}";
            var editorPersonaId = _editorPersonaId;
            var actual = new List<ResidentPortalMigrationUser>()
            {
                new ResidentPortalMigrationUser() { FirstName = "Person", LastName = "1", Email = "person1@test.com",   Username = "person1" },
                new ResidentPortalMigrationUser() { FirstName = "Person", LastName = "2", Email = "person2@test.com", Username = "person2", Properties = new List<MigrationProperty>(){
                    new MigrationProperty() { PropertyInstanceSourceId = "1" }, new MigrationProperty() { PropertyInstanceSourceId = "2" }
                } },
                new ResidentPortalMigrationUser() { FirstName = "Person", LastName = "3", Email = "person3@test.com", Username = "person3" },
                new ResidentPortalMigrationUser() { FirstName = "Person", LastName = "4", Email = "person4@test.com", Username = "person4" },
                new ResidentPortalMigrationUser() { FirstName = "Person", LastName = "5", Email = "person5@test.com", Username = "person5" }
            };
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(actual))
            };

            mockHttpMessageHandler
                .Setup(HttpMethod.Get, url, userResponse);

            IManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(
                editorRealPageId: _editorRealPageId,
                residentPortalEditorUser: _residentPortalEditorUser,
                residentPortalUser: _residentPortalUser,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                manageElectronicAddress: null,
                userClaim: _userClaims,
                messageHandler: mockHttpMessageHandler.Object,
                repository: mockRepository.Object);

            //Act
            var expected = manageProductResidentPortal.GetMigrationUsers(editorPersonaId, dataFilter);

            //Assert
            Assert.IsType<MigrationUser>(expected.Records[0]);
            Assert.True(expected.Records.Count == totalRecords);
            Assert.Same(expected.ErrorReason, "");
        }

        [Fact]
        public void UpdateUsersMigrationStatus_GivenEditorIdUserIdAndMigrateUser_ShouldReturnSuccessMessage()
        {
            //Arrange
            Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
            Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
            Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
            Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();

            IList<CustomerCompanyMap> companyMapList = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CompanyInstanceSourceId = _blueBookId.ToString(),
                    Source = BlueBookProductConstants.ResidentPortal
                }
            };

            _editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.Is<string>(l => l == "AB"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                 ))
                 .Returns(companyMapList);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_productInternalSettings);
            
            mockRepository
                .Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, 17))))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                 ))
                 .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                 ))
                 .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                 ))
                 .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                 ))
                 .Returns(_userPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.ResidentPortal));

            var migrateUsers = new List<MigrateUser>()
            {
                new MigrateUser(){
                    UserId = "123456",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = true
                },
                new MigrateUser(){
                    UserId = "123457",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = true
                }
            };
            var expected = new MigrateResponse()
            {
                Message = "Success",
                Status = true
            };
            var url = $"{_mtApiEndPoint}/{_blueBookId}/migrate-users?app_id={_appId}&app_key={_appKey}";
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(expected))
            };

            mockHttpMessageHandler.Setup(HttpMethod.Put, url, userResponse);
			
            IManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(
                editorRealPageId: _editorRealPageId,
                residentPortalEditorUser: _residentPortalEditorUser,
                residentPortalUser: _residentPortalUser,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                manageElectronicAddress: null,
                userClaim: _userClaims,
                messageHandler: mockHttpMessageHandler.Object,
                repository: mockRepository.Object);

            //Act
            var actual = manageProductResidentPortal.UpdateUsersMigrationStatus(_editorPersonaId, migrateUsers);

            //Assert
            Assert.Equal(actual.Message, expected.Message);
            Assert.True(actual.Status);
        }
        #endregion
    }
}

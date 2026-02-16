using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.User;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Ops;

namespace UnifiedLogin.LandingAPIEnterprise.Services
{
    /// <summary>
    /// Service for building and converting user profile data
    /// </summary>
    public interface IUserProfileService
    {
        ProfileDetail BuildProfileFromDto(UserProductDetailsDto userProductDetailsDto, IList<CustomFieldValue> userCustomFields, DefaultUserClaim userClaims, 
            IManageOrganization manageOrganization, IManagePersona managePersona,  IProductRepository productRepository, IManageProductPanel manageProductPanel);
        //IUserLoginLogic userLoginLogic,
        UserProductDetails GetUserBusinessObject(UserProductDetailsDto userProductDetailsDto, DefaultUserClaim userClaims);
    }

    public class UserProfileService : IUserProfileService
    {
        private readonly IManageUserLogin _userLoginLogic;

        public UserProfileService(IManageUserLogin userLoginLogic)
        {
            _userLoginLogic = userLoginLogic;
        }

        public ProfileDetail BuildProfileFromDto(
            UserProductDetailsDto userProductDetailsDto,
            IList<CustomFieldValue> userCustomFields,
            DefaultUserClaim userClaims,
            IManageOrganization manageOrganization,
            IManagePersona managePersona,
           // IUserLoginLogic userLoginLogic,
            IProductRepository productRepository,
            IManageProductPanel manageProductPanel)
        {
            var profileDetail = InitializeProfileDetail(userProductDetailsDto, userCustomFields);

            if (userProductDetailsDto.UserProfileDetails.UnityRealPageUserId != Guid.Empty)
            {
                BuildExistingUserProfile(profileDetail, userProductDetailsDto, userClaims, managePersona, manageOrganization);
            }
            else
            {
                BuildNewUserProfile(profileDetail, userProductDetailsDto);
            }

            AddPhoneNumber(profileDetail, userProductDetailsDto.UserProfileDetails.Phone);
            AddProducts(profileDetail, userProductDetailsDto, userClaims, productRepository, manageProductPanel);

            return profileDetail;
        }

        public UserProductDetails GetUserBusinessObject(UserProductDetailsDto userProductDetailsDto, DefaultUserClaim userClaims)
        {
            var userProductDetails = new UserProductDetails
            {
                EditorRealPageId = userClaims.UserRealPageGuid,
                UserProfileDetails = MapUserData(userProductDetailsDto, userClaims),
                ProductList = MapProductList(userProductDetailsDto)
            };

            return userProductDetails;
        }

        #region Private Helper Methods

        private ProfileDetail InitializeProfileDetail(UserProductDetailsDto userProductDetailsDto, IList<CustomFieldValue> userCustomFields)
        {
            return new ProfileDetail
            {
                CustomFields = userCustomFields ?? new List<CustomFieldValue>(),
                productBatch = new List<ProductBatch>(),
                userLogin = new UserLogin(),
                ExternalUserRelationship = GetUserRelationship(userProductDetailsDto.UserProfileDetails.UserType),
                organization = new List<Organization>(),
                Persona = new List<Persona>(),
                Suffix = userProductDetailsDto.UserProfileDetails.Suffix,
                Title = userProductDetailsDto.UserProfileDetails.Title,
                UserTypeId = GetGbUserType(userProductDetailsDto.UserProfileDetails.UserType),
                FirstName = userProductDetailsDto.UserProfileDetails.FirstName,
                LastName = userProductDetailsDto.UserProfileDetails.LastName,
                MiddleName = userProductDetailsDto.UserProfileDetails.MiddleName,
                CreateUserSourceType = CreateUserSourceType.UnifiedPlatform,
                NotificationEmail = userProductDetailsDto.UserProfileDetails.Email,
                EmployeeId = userProductDetailsDto.UserProfileDetails.EmployeeId
            };
        }

        private void BuildNewUserProfile(ProfileDetail profileDetail, UserProductDetailsDto userProductDetailsDto)
        {
            profileDetail.userLogin.ThruDate = null;
            profileDetail.userLogin.IsActive = true;
            profileDetail.userLogin.IsPending = false;
            profileDetail.userLogin.IsExpired = false;
            profileDetail.userLogin.FromDate = userProductDetailsDto.UserProfileDetails.UserEffectiveDate;
            profileDetail.userLogin.Is3rdPartyIDP = userProductDetailsDto.UserProfileDetails.IsExternalIdp;
            profileDetail.Password = userProductDetailsDto.UserProfileDetails.Password;
            profileDetail.userLogin.doNotForceChangePassword = userProductDetailsDto.UserProfileDetails.doNotForceChangePassword;
            profileDetail.userLogin.LoginName = userProductDetailsDto.UserProfileDetails.LoginName;
        }

        private void BuildExistingUserProfile(ProfileDetail profileDetail, UserProductDetailsDto userProductDetailsDto, 
            DefaultUserClaim userClaims, IManagePersona managePersona, IManageOrganization manageOrganization)
        {
            profileDetail.userLogin = _userLoginLogic.GetUserLogin(userProductDetailsDto.UserProfileDetails.UnityRealPageUserId, 0);
            profileDetail.organization.Add(manageOrganization.GetOrganization(Guid.Empty, userClaims.OrganizationPartyId));
            profileDetail.Persona.Add(managePersona.GetActivePersona(userProductDetailsDto.UserProfileDetails.UnityRealPageUserId));
            profileDetail.PartyId = profileDetail.userLogin.PartyId;
            profileDetail.RealPageId = userProductDetailsDto.UserProfileDetails.UnityRealPageUserId;
            profileDetail.userLogin.LoginName = userProductDetailsDto.UserProfileDetails.LoginName;
        }

        private void AddPhoneNumber(ProfileDetail profileDetail, string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return;

            profileDetail.TelecommunicationNumber = new List<TelecommunicationNumber>
            {
                new TelecommunicationNumber
                {
                    CountryCode = "+1",
                    ISOCode = "US",
                    PhoneNumber = phoneNumber,
                    IsDeleted = false,
                    IsPreferred = false,
                    IsDefault = false,
                    AreaCode = string.Empty,
                    contactMechanismUsageType = new ContactMechanismUsageType 
                    { 
                        ContactMechanismUsageTypeId = 203, 
                        Name = string.Empty 
                    },
                    PartyContactMechanismId = 0,
                    ContactMechanismId = 0
                }
            };
        }

        private void AddProducts(ProfileDetail profileDetail, UserProductDetailsDto userProductDetailsDto, 
            DefaultUserClaim userClaims, IProductRepository productRepository, IManageProductPanel manageProductPanel)
        {
            if (userProductDetailsDto.ProductList == null || !userProductDetailsDto.ProductList.Any())
                return;

            var products = productRepository.GetAllProducts();

            foreach (var pl in userProductDetailsDto.ProductList)
            {
                var productBatch = new ProductBatch
                {
                    //InputJson = new RolePropertyList(),
                    ProductId = ProductEnumHelper.GetProductIdByProductCode(pl.ProductCode, products),
                    StatusTypeId = (int)ProductBatchStatusType.Waiting,
                    RetryCount = 0,
                    InputJson = new RolePropertyList
                    {
                        RoleList = pl.RolesAssigned,
                        PropertyList = pl.PropertiesAssigned,
                        IsAssigned = pl.IsAssigned
                    }
                };

                if (productBatch.ProductId == (int)ProductEnum.OpsBuyer && userProductDetailsDto.UserProfileDetails.UnityRealPageUserId != Guid.Empty)
                {
                    var response = manageProductPanel.GetProductProperties(userClaims.PersonaId, profileDetail.Persona[0].PersonaId, productBatch.ProductId, null);
                    var removeProp = response.Records?.Cast<AssetGroup>()?.Where(c => c.IsAssigned)?.Select(y => y.ID)?.ToList();
                    productBatch.InputJson.RemovedPropertyList = removeProp ?? new List<string>();
                }

                profileDetail.productBatch.Add(productBatch);
            }
        }

        private UserData MapUserData(UserProductDetailsDto userProductDetailsDto, DefaultUserClaim userClaims)
        {
            return new UserData
            {
                UserRealPageId = userProductDetailsDto.UserProfileDetails.UnityRealPageUserId,
                MiddleName = userProductDetailsDto.UserProfileDetails.MiddleName,
                Password = userProductDetailsDto.UserProfileDetails.Password,
                LoginName = userProductDetailsDto.UserProfileDetails.LoginName,
                Title = userProductDetailsDto.UserProfileDetails.Title,
                Email = userProductDetailsDto.UserProfileDetails.Email,
                FirstName = userProductDetailsDto.UserProfileDetails.FirstName,
                UserType = GetGbUserType(userProductDetailsDto.UserProfileDetails.UserType),
                IsExternalIdp = userProductDetailsDto.UserProfileDetails.IsExternalIdp,
                LastName = userProductDetailsDto.UserProfileDetails.LastName,
                OrganizationRealPageId = userClaims.OrganizationRealPageGuid,
                OrganizationPartyId = userClaims.OrganizationPartyId,
                Phone = userProductDetailsDto.UserProfileDetails.Phone,
                UserEffectiveDate = userProductDetailsDto.UserProfileDetails.UserEffectiveDate,
                UserExpirationDate = userProductDetailsDto.UserProfileDetails.UserExpirationDate,
                CreateUserSourceType = CreateUserSourceType.RPX.ToString(),
                Suffix = userProductDetailsDto.UserProfileDetails.Suffix,
                CustomFields = userProductDetailsDto.UserProfileDetails.CustomFields,
                EmployeeId = userProductDetailsDto.UserProfileDetails.EmployeeId,
                SendInvitationEmail = userProductDetailsDto.UserProfileDetails.SendInvitationEmail
            };
        }

        private List<ProductDetail> MapProductList(UserProductDetailsDto userProductDetailsDto)
        {
            var productList = new List<ProductDetail>();

            if (userProductDetailsDto.ProductList == null)
                return productList;

            foreach (var product in userProductDetailsDto.ProductList)
            {
                productList.Add(new ProductDetail
                {
                    ProductCode = product.ProductCode,
                    AdditionalFields = product.AdditionalFields,
                    PropertiesAssigned = product.PropertiesAssigned,
                    RegionsAssigned = product.RegionsAssigned,
                    RolesAssigned = product.RolesAssigned,
                    IsAssigned = product.IsAssigned
                });
            }

            return productList;
        }

        private int GetGbUserType(UserTypeDto userTypeDto)
        {
            return userTypeDto switch
            {
                UserTypeDto.Regular => (int)UserRoleType.User,
                UserTypeDto.NoEmail => (int)UserRoleType.UserNoEmail,
                UserTypeDto.Employee => (int)UserRoleType.RealPageEmployee,
                UserTypeDto.External => (int)UserRoleType.ExternalUser,
                UserTypeDto.SuperUser => (int)UserRoleType.SuperUser,
                _ => throw new ArgumentOutOfRangeException(nameof(userTypeDto), userTypeDto, null)
            };
        }

        private ExternalUserRelationship GetUserRelationship(UserTypeDto userTypeDto)
        {
            return userTypeDto switch
            {
                UserTypeDto.Regular => new ExternalUserRelationship { ThirdPartyRelationShipId = 4, ThirdPartyRelationShip = "4" },
                UserTypeDto.NoEmail => new ExternalUserRelationship { ThirdPartyRelationShipId = 6, ThirdPartyRelationShip = "6" },
                UserTypeDto.SuperUser => new ExternalUserRelationship { ThirdPartyRelationShipId = 8, ThirdPartyRelationShip = "8" },
                UserTypeDto.External => new ExternalUserRelationship { ThirdPartyRelationShipId = 5, ThirdPartyRelationShip = "5" },
                UserTypeDto.Employee => new ExternalUserRelationship { ThirdPartyRelationShipId = 9, ThirdPartyRelationShip = "9" },
                _ => throw new ArgumentOutOfRangeException(nameof(userTypeDto), userTypeDto, null)
            };
        }

        #endregion
    }
}

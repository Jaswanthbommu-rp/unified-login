using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers
{
	public class DataCollector : IDataCollector
	{
		private const string PRODUCT_SETTINGTYPE_STATUS = "ProductStatus";

		private readonly IProductRepository _productRepository = new ProductRepository();
		private readonly ISamlRepository _samlRepository = new SamlRepository();
		private readonly IUserRepository _userRepository = new UserRepository();
		private readonly ITelecommunicationNumberRepository _telecommunicationNumberRepository = new TelecommunicationNumberRepository();

        /// <summary>
        /// Get Product Company Map
        /// </summary>
        public CustomerCompanyMap GetProductCompanyMap(string blueBookProductCode, int booksMasterId, DefaultUserClaim userClaims, string domain)
        {
            try
            {
                IManageBlueBook blueBook = new ManageBlueBook(userClaims);

                IList<CustomerCompanyMap> companyProductList = blueBook.GetCompanyMap(userClaims.OrganizationRealPageGuid, booksMasterId, source: blueBookProductCode.ToUpper(), domain: domain);
                if (companyProductList == null)
                {
                    companyProductList = new List<CustomerCompanyMap>();
                }

                if (companyProductList.Any(a => a.Source.Equals(blueBookProductCode, StringComparison.OrdinalIgnoreCase)))
                {
                    return (from a in companyProductList where a.Source.Equals(blueBookProductCode, StringComparison.OrdinalIgnoreCase) select a).FirstOrDefault();
                }
                
                return new CustomerCompanyMap();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
		/// Get BlueBook Product map
		/// </summary>
		public GbProductMap GetBlueBookProductMap(int productId)
		{
			// Get bluebook product code
			return _productRepository.GetBooksMasterProductDetail(productId);
		}

        public void CreateProductUserInGreenBook(long subjectPersonaId, dynamic userResult, int productId, IntegrationProductUser productUser)
        {
            string newid = userResult.userId != null ? (string)userResult.userId : (string)userResult.UserId;
            string newProductLoginName = userResult.loginName != null ? (string)userResult.loginName : productUser.LoginName;

            if (string.IsNullOrEmpty(newid))
                throw new Exception($"Unable to get userId from response. userResult-{userResult}");
            var samlProductAttributes = _samlRepository.GetSamlProductAttributes(productId);

            var productInternalSettingList = _productRepository.GetProductInternalSettings(productId);
            var assignSamlAttributeBySetting = productInternalSettingList.FirstOrDefault(a => a.Name.Equals("AssignSamlAttributeBySetting", StringComparison.OrdinalIgnoreCase))?.Value;
            if (assignSamlAttributeBySetting != null && assignSamlAttributeBySetting.Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                if (samlProductAttributes != null && samlProductAttributes.Any())
                {
                    foreach (var attr in samlProductAttributes)
                    {
                        if (!Enum.TryParse<SamlAttributeEnum>(attr.SamlAttributeName, true, out var samlEnum))
                        {
                            continue; // invalid name, skip
                        }
                        string samlAttributeValue = attr.DisplayName;
                        if (samlEnum.Equals(SamlAttributeEnum.productUsername))
                            samlAttributeValue = newProductLoginName;
                        else if (samlEnum.Equals(SamlAttributeEnum.UserId))
                            samlAttributeValue = newid;
                        else if (samlEnum.Equals(SamlAttributeEnum.organization_id))
                        {
                            var organizationId = productInternalSettingList.FirstOrDefault(a => a.Name.Equals("OrganizationId", StringComparison.OrdinalIgnoreCase))?.Value;
                            if (organizationId != null)
                            {
                                samlAttributeValue = organizationId;
                            }
                            else
                                continue;
                        }
                        else if (samlEnum.Equals(SamlAttributeEnum.portal_id))
                        {
                            var portalId = productInternalSettingList.FirstOrDefault(a => a.Name.Equals("PortalId", StringComparison.OrdinalIgnoreCase))?.Value;
                            if (portalId != null)
                                samlAttributeValue = portalId;
                            else
                                continue;
                        }
                        else if (samlEnum.Equals(SamlAttributeEnum.RoleCode))
                            samlAttributeValue = productUser.RoleType;
                        CreateSamlUserAttribute(subjectPersonaId, productId, samlEnum, samlAttributeValue);

                    }
                }

            }
            else
            {
                CreateSamlUserAttribute(subjectPersonaId, productId, SamlAttributeEnum.productUsername, newProductLoginName);
                CreateSamlUserAttribute(subjectPersonaId, productId, SamlAttributeEnum.UserId, newid);
            }
            //WriteToDiagnosticLog("ManageProductVendorServices.CreateProductUserInGreenBook - Create user Success. Set product status to Success");
            UpdateProductSettingProductStatus(subjectPersonaId, PRODUCT_SETTINGTYPE_STATUS, productId, (int)ProductBatchStatusType.Success);
        }

        public void CreateSamlUserAttribute(long subjectPersonaId, int productId, SamlAttributeEnum samlAttributeEnum, string value)
		{
			if (string.IsNullOrEmpty(value))
				throw new Exception($"Unable to get value from response.");

			_samlRepository.CreateSamlUserAttribute(subjectPersonaId, productId, samlAttributeEnum, value);
		}

		/// <summary>
		/// Updates Saml User Attribute
		/// </summary>
		public void UpdateSamlUserAttribute(long personaId, int productId, SamlAttributeEnum attributeType, string newValue)
		{
			if (string.IsNullOrEmpty(newValue))
				throw new Exception($"Unable to get newValue from response.");

			Dictionary<SamlAttributeEnum, string> settingList = new Dictionary<SamlAttributeEnum, string>
			{
				{attributeType, newValue}
			};

			UpdateSamlUserAttributes(personaId, settingList, productId);
		}

		/// <summary>
		/// Used to add/update a list of product settings for the given product and persona
		/// </summary> 
		private void UpdateSamlUserAttributes(long personaId, Dictionary<SamlAttributeEnum, string> settingList, int productId)
		{
			IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(personaId, productId);
			if (settingList.Count > 0)
			{
				foreach (KeyValuePair<SamlAttributeEnum, string> setting in settingList)
				{
					if (productAttributes.Any(a => a.SamlAttributeId == (int)setting.Key))
					{
						SamlAttributes attributeToReplace = (from a in productAttributes where a.SamlAttributeId == (int)setting.Key select a).FirstOrDefault();
						if (attributeToReplace != null)
						{
							attributeToReplace.Value = setting.Value;
							_samlRepository.UpdateSamlUserAttribute(attributeToReplace);
						}
					}
				}
			}
		}

		public UserDetails GetUserDetailsByPersona(long personaId, int productId)
		{
			// Get user details
			var userDetails = _userRepository.GetUserDetails(personaId);

			//Get the Person's phone numbers
			var telecommunicationPhoneNumbers = _telecommunicationNumberRepository.ListTelecommunicationNumberForPerson(userDetails.UserRealPageId , string.Empty);

			userDetails.PhoneNumbers = telecommunicationPhoneNumbers.Select(x => $"{x.AreaCode}{x.PhoneNumber}").ToList();

			// get user saml details & append to userDetails
			GetUserSamlDetails(userDetails, productId);

			return userDetails;
		}

        public AdUserDetail GetAzureUserDetails(long userId)
        {
            return _userRepository.GetAzureUserDetails(userId);
        }

        public List<AdGroup> GetAdGroupsForUser(long personaId)
        {
            return _productRepository.GetAdGroupsForUser(personaId);
        }

		public IList<EmployeeProductMapping> GetEmployeeProductADGroupMapping(long personaId, int productId)
        {
			return _userRepository.GetEmployeeProductADGroupMapping(personaId, productId);
		}

		private void GetUserSamlDetails(UserDetails userDetails, int productId)
		{
			IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(userDetails.PersonaId, productId);

			if (productAttributes != null && productAttributes.Any())
			{
				// the  user making the change, get the Company from the user
				if (productAttributes.Any(a => a.Name.Equals("ProductUserName", StringComparison.OrdinalIgnoreCase)))
				{
					userDetails.ProductUserName =
						(from a in productAttributes where a.Name.Equals("ProductUserName", StringComparison.OrdinalIgnoreCase) select a.Value)
							.FirstOrDefault();
				}

				if (productAttributes.Any(a => a.Name.Equals("UserId", StringComparison.OrdinalIgnoreCase)))
				{
					userDetails.ProductUserId =
						(from a in productAttributes where a.Name.Equals("UserId", StringComparison.OrdinalIgnoreCase) select a.Value).FirstOrDefault();
				}
			}
		}

		public void UpdateProductSettingProductStatus<T>(long subjectPersonaId, string settingType, int productId, T value)
		{
			// add the new status flag to the product before we start
			IList<ProductSettingType> productSettingTypes = _productRepository.ListProductSettingType();

			// get the id for ProductStatus type
			if (productSettingTypes.Any(a => a.Name.ToUpper() == settingType.ToUpper()))
			{
				int productStatusTypeId = (from a in productSettingTypes where a.Name.ToUpper() == settingType.ToUpper() select a.ProductSettingTypeId).FirstOrDefault();
				_productRepository.CreateProductSetting(subjectPersonaId, productId, productStatusTypeId, value.ToString());
			}
		}

        public void AddUpdateEmployeeProductADGroupMapping(long personaId, int productId, int adGroupId)
        {
            _userRepository.AddUpdateEmployeeProductADGroupMapping(personaId, productId, adGroupId);
        }
	}
}
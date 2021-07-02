using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using IdentityModel.Client;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
	//RealPage UtilityManagement
	public class ManageProductRum : ManageProductBase, IManageProductRum
    {
        #region Private members

        private IProductInternalSettingRepository _productInternalSettingRepository;
        private string _clientId;
        private string _apiEndPoint;
        private string _apiSecret;
        private string _accessToken;
        private string _tokenEndPoint;
        private string _nwpIssueUri;
        TokenClient _tokenClient;
        private DefaultUserClaim _userClaims;

        #endregion

        #region Ctor
        /// <summary>
		/// 
		/// </summary>
		/// <param name="userClaims"></param>
        public ManageProductRum(DefaultUserClaim userClaims) : base((int)ProductEnum.UtilityManagement, userClaims, null, null)
        {
            WriteToDiagnosticLog("ManageProductRum.Ctor - Getting Product settings.");
            _productId = (int)ProductEnum.UtilityManagement;
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _editorRealPageId = userClaims.UserRealPageGuid;
	        _userClaims = userClaims;

            _blueBook = new ManageBlueBook(userClaims);

            _apiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
            _apiSecret = _productInternalSettingList.First(a => a.Name.ToUpper() == "APISECRET").Value;
            _clientId = _productInternalSettingList.First(a => a.Name.ToUpper() == "CLIENTID").Value;
            _nwpIssueUri = _productInternalSettingList.First(a => a.Name.ToUpper() == "TOKENURL").Value;

            WriteToDiagnosticLog($"ManageProductRum.Ctor - apiEndPoiint - {_apiEndPoint}");
            WriteToDiagnosticLog($"ManageProductRum.Ctor - apiSecret - {_apiSecret}");
            WriteToDiagnosticLog($"ManageProductRum.Ctor - clientId - {_clientId}");
            WriteToDiagnosticLog($"ManageProductRum.Ctor - nwpIssueUri - {_nwpIssueUri}");
            WriteToDiagnosticLog("ManageProductRum.Ctor - Received Product settings; getting token.");

            _tokenClient = new TokenClient($"{_nwpIssueUri}/connect/token", _clientId, _apiSecret);


            GetToken();
        }

        /// <summary>
		/// 
		/// </summary>
		/// <param name="editorRealPageId"></param>
		/// <param name="userClaims"></param>
		/// <param name="messageHandler"></param>
		/// <param name="tokenMessageHandler"></param>
		/// <param name="productInternalSettingRepository"></param>
		/// <param name="managePersona"></param>
		/// <param name="samlRepository"></param>
		/// <param name="blueBook"></param>
        public ManageProductRum(Guid editorRealPageId, DefaultUserClaim userClaims, HttpMessageHandler messageHandler, HttpMessageHandler tokenMessageHandler, 
            IProductInternalSettingRepository productInternalSettingRepository, IManagePersona managePersona, 
            ISamlRepository samlRepository, IManageBlueBook blueBook, IProductRepository productRepository)
             : base((int)ProductEnum.UtilityManagement, userClaims, productInternalSettingRepository, productRepository)
        {
            _editorRealPageId = editorRealPageId;
            _productInternalSettingRepository = productInternalSettingRepository;
            _blueBook = blueBook;
            _managePersona = managePersona;
            _samlRepository = samlRepository;
            _messageHandler = messageHandler;
	        _userClaims = userClaims;
            _productRepository = productRepository;
            _apiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
            _apiSecret = _productInternalSettingList.First(a => a.Name.ToUpper() == "APISECRET").Value;
            _clientId = _productInternalSettingList.First(a => a.Name.ToUpper() == "CLIENTID").Value;
            _nwpIssueUri = _productInternalSettingList.First(a => a.Name.ToUpper() == "TOKENURL").Value;

            WriteToDiagnosticLog($"ManageProductRum.Ctor - apiEndPoiint - {_apiEndPoint}");
            WriteToDiagnosticLog($"ManageProductRum.Ctor - apiSecret - {_apiSecret}");
            WriteToDiagnosticLog($"ManageProductRum.Ctor - clientId - {_clientId}");
            WriteToDiagnosticLog($"ManageProductRum.Ctor - nwpIssueUri - {_nwpIssueUri}");
            WriteToDiagnosticLog("ManageProductRum.Ctor - Received Product settings; getting token.");

            _tokenClient = new TokenClient($"{_nwpIssueUri}/connect/token", _clientId, _apiSecret, tokenMessageHandler);
            _client = new HttpClient(messageHandler, false);

            GetToken();
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Get Properties
        /// </summary>
        public ListResponse GetPropertyGroups(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            WriteToDiagnosticLog(
              $"ManageProductRum.GetPropertyGroups at beginning of method for user with editorPersona id - {editorPersonaId}");

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor

                if (result.IsError)
                {
                    WriteToErrorLog(
                        $"ManageProductRum.GetProperties.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                    return result;
                }

                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);

                if (companyInstanceSourceId == 0)
                {
                    WriteToErrorLog(
                        $"ManageProductRum.GetPropertyGroups-GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}.");
                    return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };
                }
                WriteToDiagnosticLog($"RUM - GetPropertyGroups-GetProductCompanyInstanceId - Found blue book company instance source id - {companyInstanceSourceId}  for user editorPersona id -{editorPersonaId}");


                // get access items from Rum product
                var groups = GetRumPropertiesData(companyInstanceSourceId, "GM");

                var allPropertyGroups = groups as IList<RumPropertyGroup> ?? groups.ToList();

                if (allPropertyGroups == null || allPropertyGroups.Count == 0)
                {
                    WriteToErrorLog($"ManageProductRum.GetPropertyGroups-no properties received from product for user with editorPersona id - {editorPersonaId}.");

                    response.IsError = true;
                    response.ErrorReason = "No properties received from product.";
                    return response;
                }

                if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId)) // Called during updating Existing User
                {
                    WriteToDiagnosticLog(
                         $"ManageProductRum.GetPropertyGroups-MergePropertiesWithGreenbook calling....for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
                    response = MergeRumPropertiesWithGreenbook(allPropertyGroups, userPersonaId);
                    WriteToDiagnosticLog(
                           $"ManageProductRum.GetPropertyGroups -MergePropertiesWithGreenbook completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
                }
                else // Called during creating a new User
                {
                    response = new ListResponse()
                    {
                        Records = allPropertyGroups.Cast<object>().ToList(),
                        TotalRows = allPropertyGroups.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                }

                WriteToDiagnosticLog($"Exiting ManageProductRum.GetPropertyGroups method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId}.");
            }
            catch (Exception ex)
            {
                response = new ListResponse
                {
                    IsError = true
                };

                if (ex is BlueBookException)
                {
                    response.ErrorReason = ex.Message;
                }
                else
                {
                    response.ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage;
                }

                WriteToErrorLog($"ManageProductRum.GetPropertyGroups Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
            }

            return response;
        }

        // <summary>
        /// Used to get properties  
        /// </summary>
        /// <param name="editorPersonaId">The persona id of the user making the request</param>
        /// <param name="userPersonaId">The persona id of the user being changed</param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
			Dictionary<string, object> logData = new Dictionary<string, object>();
			var result = new ListResponse();
			IList<RumPropertyGroup> rumProperties = new List<RumPropertyGroup>();
			WriteToDiagnosticLog(
			  $"ManageProductRum.GetProperties - at begining of method for user with editorPersona id - {editorPersonaId}");

			try
			{
				result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO: need to refactor
				if (result.IsError)
				{
					WriteToErrorLog(
						$"ManageProductRum.GetProperties.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
					return result;
				}

				string rumCompanyId = "";

				// get the PMCID from BlueBook because the user doesn't have the PMCID for Marketing Center yet
				WriteToDiagnosticLog("GetRUMPMCIDFromPersona - Getting info from BlueBook.GetCompanyMap");
                //IList<CompanyMap> companyMap = _blueBook.GetCompanyMap(_editorPersona.Organization.BooksMasterId, BlueBookProductConstants.UtilityManagement);
                IList<CustomerCompanyMap> companyMap = _blueBook.GetCompanyMap(_editorPersona.Organization.RealPageId, _editorPersona.Organization.BooksCustomerMasterId, source: BlueBookProductConstants.UtilityManagement, domain: _editorPersona.Organization.OrganizationDomain.Name);
                WriteToDiagnosticLog("GetRUMPMCIDFromPersona - Done getting info from BlueBook.GetCompanyMap");
				if (companyMap != null && companyMap.Count > 0 && companyMap.Any(a => a.Source.ToUpper() == BlueBookProductConstants.UtilityManagement))
				{
					WriteToDiagnosticLog("GetRUMPMCIDFromPersona - Getting PMC ID from BlueBook result");
					rumCompanyId = companyMap.First(a => a.Source.ToUpper() == BlueBookProductConstants.UtilityManagement).CompanyInstanceSourceId;
					WriteToDiagnosticLog("GetRUMPMCIDFromPersona - Found PMC ID from BlueBook result: {rumCompanyId}");
				}


				var url = $"{ _apiEndPoint}/identity/Property?companyId= {rumCompanyId} ";
				logData = new Dictionary<string, object>();
				logData.Add("url", url);
				WriteToDiagnosticLog("GetProperties - Posting to url", logData);

				var propertyList = GetResultFromApi<IList<ProductPropertyMap>>(_accessToken, url);

                if (propertyList != null && propertyList.Count > 0)
                {
                    WriteToDiagnosticLog($"ManageProductRum.GetProperties-GetPropertyInstance - Found total {propertyList.Count} properties with blue book company instance id {rumCompanyId} for user with editorPersona id - {editorPersonaId}.");

                    foreach (var property in propertyList)
                    {
                        RumPropertyGroup rpg = new RumPropertyGroup
                        {
                            Id = Convert.ToInt32(property.PropertyId),
                            Name = property.PropertyName,
                            State = property.State,
                            IsAssigned = false
                        };
                        rumProperties.Add(rpg);
                    }
                    // need to do a filter on the result
                    if (userPersonaId != 0 && (_productUserId != null && _productUserId.Length > 0)) // Called during updating Existing User
                    {
                        WriteToDiagnosticLog(
                            $"ManageProductRum.GetProperties- calling MergeProductPropertiesWithGreenbook....for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
                        result = MergeRumPropertiesWithGreenbook(rumProperties, userPersonaId);
                        WriteToDiagnosticLog(
                             $"ManageProductRum.GetProperties-MergeProductPropertiesWithGreenbook completed for user with editorPersona id -{editorPersonaId}.");
                    }
                    else
                    {
                        result = new ListResponse() // Called during creating a new User
                        {
                            Records = rumProperties.Cast<object>().ToList(),
                            TotalRows = rumProperties.Count,
                            RowsPerPage = rumProperties.Count,
                            TotalPages = 1,
                            ErrorReason = string.Empty
                        };
                    }
                }
                else 
                {
                    result = new ListResponse
                    {
                        IsError = true,
                        ErrorReason = CommonMessageConstants.PropertyErrorMessage
                    };

                }
            }
			catch (Exception ex)
			{
                result = new ListResponse
                {
                    IsError = true
                };

                if (ex is BlueBookException)
                {
                    result.ErrorReason = ex.Message;
                }
                else
                {
                    result.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
                }

				WriteToErrorLog(
					$"ManageProductRum.GetProperties - There was a problem getting the properties for user with editorPersona id - {editorPersonaId}.",
					exception: ex);
			}

			return result;
		}

        /// <summary>
        /// Get Regions
        /// </summary>
        public ListResponse GetRegions(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            WriteToDiagnosticLog(
              $"ManageProductRum.GetRegions at beginning of method for user with editorPersona id - {editorPersonaId}");

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor

                if (result.IsError)
                {
                    WriteToErrorLog(
                        $"ManageProductRum.GetRegions.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                    return result;
                }

                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
                if (companyInstanceSourceId == 0)
                {
                    WriteToErrorLog(
                        $"ManageProductRum.GetRegions-GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}.");
                    return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };
                }

                // get access groups from on-site product
                var allRegions = GetRumPropertiesData(companyInstanceSourceId, "RM");

                if (allRegions == null)
                {
                    WriteToErrorLog($"ManageProductRum.GetRegions-no properties received from product for user with editorPersona id - {editorPersonaId}.");

                    response.IsError = true;
                    response.ErrorReason = CommonMessageConstants.RegionErrorMessage;
                    return response;
                }

                if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId)) // Called during updating Existing User
                {
                    WriteToDiagnosticLog(
                         $"ManageProductRum.GetRegions-MergeRegionsWithGreenbook calling....for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
                    response = MergeRumPropertiesWithGreenbook(allRegions, userPersonaId);
                    WriteToDiagnosticLog(
                           $"ManageProductRum.GetRegions-MergeRegionsWithGreenbook completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
                }
                else // Called during creating a new User
                {
                    response = new ListResponse()
                    {
                        Records = allRegions.Cast<object>().ToList(),
                        TotalRows = allRegions.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                }

                WriteToDiagnosticLog($"Exiting ManageProductRum.GetRegions method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId}.");
            }
            catch (Exception ex)
            {
                response = new ListResponse
                {
                    IsError = true
                };

                if (ex is BlueBookException)
                {
                    response.ErrorReason = ex.Message;
                }
                else
                {
                    response.ErrorReason = CommonMessageConstants.RegionErrorMessage;
                }
                WriteToErrorLog($"ManageProductRum.GetRegions Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
            }

            return response;
        }

        /// <summary>
        ///Get roles
        /// </summary>
        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            WriteToDiagnosticLog(
               $"ManageProductRum.GetRoles at beginning of method for user with editorPersona id - {editorPersonaId}");

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor

                if (result.IsError)
                {
                    WriteToErrorLog(
                        $"ManageProductRum.GetRoles.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                    return result;
                }

                //int companyInstanceSourceId = 279; // to get sample groups 
                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);

                // get roles from rum product
                var allRoles = GetRumRoles(companyInstanceSourceId);

                if (allRoles == null)
                {
                    WriteToErrorLog($"ManageProductRum.GetRoles-no access groups (roles) received from product for user with editorPersona id - {editorPersonaId}.");

                    response.IsError = true;
                    response.ErrorReason = "No User Access groups (roles) received from product.";
                    return response;
                }

                if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId)) // Called during updating Existing User
                {
                    WriteToDiagnosticLog(
                         $"ManageProductRum.GetRoles-MergeUserRolesWithProductRoles calling....for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
                    response = MergeUserRolesWithProductRoles(allRoles, userPersonaId);
                    WriteToDiagnosticLog(
                           $"ManageProductRum.GetRoles-MergeUserRolesWithProductRoles completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
                }
                else // Called during creating a new User
                {
                    response = new ListResponse()
                    {
                        Records = allRoles.Cast<object>().ToList(),
                        TotalRows = allRoles.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                }

                WriteToDiagnosticLog($"Exiting ManageProductRum.GetRoles method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId}.");
            }
            catch (Exception ex)
            {
                response = new ListResponse
                {
                    IsError = true
                };

                if (ex is BlueBookException)
                {
                    response.ErrorReason = ex.Message;
                }
                else
                {
                    response.ErrorReason = CommonMessageConstants.AdditionalRightErrorMessage;
                }
                WriteToErrorLog($"ManageProductRum.GetRoles Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
            }

            return response;
        }

        public ListResponse GetUMGlobalRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            WriteToDiagnosticLog(
               $"ManageProductRum.GetUMGlobalRoles at beginning of method for user with editorPersona id - {editorPersonaId}");

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor

                if (result.IsError)
                {
                    WriteToErrorLog(
                        $"ManageProductRum.GetUMGlobalRoles.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                    return result;
                }

                //int companyInstanceSourceId = 279; // to get sample groups 
                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
                if (companyInstanceSourceId == 0)
                {
                    WriteToErrorLog(
                        $"ManageProductRum.GetUMGlobalRoles.GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}.");
                    return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };
                }

                // get roles from rum product
                List<ProductRole> globalRoles = new List<ProductRole>();
                globalRoles.Add(new ProductRole
                {
                    ID ="PR",
                    Name = "Select Properties",
                    Description = "Select Properties",
                    IsAssigned = false
                });

                globalRoles.Add(new ProductRole
                {
                    ID = "GM",
                    Name = "Groups",
                    Description = "Groups",
                    IsAssigned = false
                });

                globalRoles.Add(new ProductRole
                {
                    ID = "PM",
                    Name = "All Properties",
                    Description = "All Properties",
                    IsAssigned = false
                });

                if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId)) // Called during updating Existing User
                {
                    WriteToDiagnosticLog(
                         $"ManageProductRum.GetUMGlobalRoles-MergeUserRolesWithProductRoles calling....for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
                    response = MergeRumGlobalRolesWithGreenbook(globalRoles, userPersonaId);
                    WriteToDiagnosticLog(
                           $"ManageProductRum.GetUMGlobalRoles-MergeUserRolesWithProductRoles completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
                }
                else // Called during creating a new User
                {
                    response = new ListResponse()
                    {
                        Records = globalRoles.Cast<object>().ToList(),
                        TotalRows = globalRoles.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                }

                WriteToDiagnosticLog($"Exiting ManageProductRum.GetUMGlobalRoles method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId}.");
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"ManageProductRum.GetUMGlobalRoles Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
                response = new ListResponse();
                response.IsError = true;

                if (ex is BlueBookException blueBookException)
                {
                    response.ErrorReason = ex.Message;
                }
                else
                {
                    response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                }
            }

            return response;
        }

        /// <summary>
        /// Unassign User
        /// </summary> 
        public string UnassignRumUser(long editorPersonaId, long userPersonaId)
        {
            var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                WriteToErrorLog(
                 $"ManageProductRum.UnassignUser - Error for user with userPersonaId:{userPersonaId}. ErrorReason-{listResponse.ErrorReason}");
                return listResponse.ErrorReason;
            }
            
            //De Activate User
            var result = DeleteRumUser(editorPersonaId, userPersonaId);            

            if (string.IsNullOrEmpty(result)) {
                //WriteDeActivatedActivityLog(editorPersonaId, userPersonaId); //commented this code to avoid double activity.
                WriteToDiagnosticLog($"ManageProductRum.UnassignUser userPersonaId:{userPersonaId}");
				UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);

				// Activity Logging
				WriteUnassignActivityLog(editorPersonaId, userPersonaId);
			}			

            return result;
        }

		/// <summary>
		/// Update User Profile
		/// </summary>
		public string UpdateUserProfile(long editorPersonaId, long userPersonaId)
		{
			string result = string.Empty;
			WriteToDiagnosticLog($"ManageProductRum.UpdateUserProfiler - Begin Update User Profile for user with editorPersona id - {editorPersonaId}.");
			try
			{
				var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
				if (listResponse.IsError)
				{
					WriteToErrorLog($"ManageProductRum.UpdateUserProfile Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}");
					return listResponse.ErrorReason;
				}

				var persona = _managePersona.GetPersona(userPersonaId);
				var realPageId = persona.RealPageId;
				var person = _managePerson.GetPerson(realPageId);
				var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

				// get the email address
				string userEmailAddress = string.Empty;
				var manageElectronicAddress = new ManageElectronicAddress();
				var addresses = manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, string.Empty);

				if (addresses != null)
				{
					if (addresses.Any(
						a =>
							a.AddressType.ToUpper() == "EMAIL"))
					{
						userEmailAddress = (from a in addresses
											where
											a.AddressType.ToUpper() == "EMAIL"
											select a.AddressString).FirstOrDefault();
					}
				}

				if (string.IsNullOrEmpty(userEmailAddress))
				{
					WriteToDiagnosticLog(
					 $"ManageProductRum.UpdateUserProfile- no email address for user with editorPersona id - {editorPersonaId}; assigning bogus email.");

					userEmailAddress = ValidateAndReturnEmailAddress(userLogin.LoginName);
				}

				RumUser rumUser = new RumUser
				{
					FirstName = person.FirstName,
					LastName = person.LastName,
					Email = userEmailAddress
				};

				using (var client = new HttpClient())
				{
					client.DefaultRequestHeaders.Clear();
					client.DefaultRequestHeaders.Authorization =
						new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

					WriteToDiagnosticLog($"ManageProductRum.UpdateUserProfile - calling product API for user with editorPersona id - {editorPersonaId}.");

					var response = client.PutAsJsonAsync($"{_apiEndPoint}/user/putuserinfo?userId={_productUserId}", rumUser).Result;

					if (response.IsSuccessStatusCode)
					{
						WriteToDiagnosticLog($"ManageProductRum.UpdateUserProfile - IsSuccessStatusCode return true for user with editorPersona id - {editorPersonaId}.");

						var jsonContent = response.Content.ReadAsStringAsync().Result;
						dynamic userResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
						if (userResult != null)
						{
							result = string.Empty;
						}
						WriteUpdateUserTypeActivityLog(editorPersonaId, person, userLogin, BatchProcessType.ProfileUpdate);
					}
					else
					{
						string errorContent = string.Empty;
						try
						{
							errorContent = response.Content.ReadAsStringAsync().Result;
						}
						catch
						{/*Ignored*/ }
						Dictionary<string, object> logData = new Dictionary<string, object>() { { "errorContent", errorContent } };
						WriteToErrorLog(
							$"ManageProductRum.UpdateUserProfile Error for user with editorPersona id - {editorPersonaId}.", logData);
						result = $"There was a problem updating user profile for user with editorPersona id - {editorPersonaId} - Error-{errorContent}.";
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"ManageProductRum.UpdateUserProfile - Error for user with editorPersona id - {editorPersonaId}", exception: ex);
				return $"Error - {ex.Message}";
			}
		}

		/// <summary>
		/// Updated to create/update a user in On Site 
		/// </summary>
		public string ManageRumUser(long editorPersonaId, long userPersonaId, RumUserPropertyRegionRole userPropertyRegionRole)
        {
            WriteToDiagnosticLog($"ManageProductRum.ManageOnSiteUser - Begin create/update user for user with editorPersona id - {editorPersonaId}.");

            try
            {
                if (userPropertyRegionRole == null)
                {
                    throw new Exception(
                        "RumUserPropertyRegionRole received null; check JSON in product batch table or parsing issue.");
                }

                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog($"ManageProductRum.ManageRumUser Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}");
                    return listResponse.ErrorReason;
                }

                
                var persona = _managePersona.GetPersona(userPersonaId);
                var realPageId = persona.RealPageId;
                var person = _managePerson.GetPerson(realPageId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);
                string userAccessType = "";
                List<int> propertiesList = new List<int> { };

                // get the email address
                string userEmailAddress = string.Empty;
                var manageElectronicAddress = new ManageElectronicAddress();
                var addresses = manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, string.Empty);

                if (addresses != null)
                {
                    if (addresses.Any(
                        a =>
                            a.AddressType.ToUpper() == "EMAIL"))
                    {
                        userEmailAddress = (from a in addresses
                                            where
                                            a.AddressType.ToUpper() == "EMAIL"
                                            select a.AddressString).FirstOrDefault();
                    }
                }

                if (string.IsNullOrEmpty(userEmailAddress))
                {
                    WriteToDiagnosticLog(
                     $"ManageProductRum.ManageRumUser- no email address for user with editorPersona id - {editorPersonaId}; assigning bogus email.");

                    userEmailAddress = ValidateAndReturnEmailAddress(userLogin.LoginName);
                }

                var productLoginName = string.IsNullOrEmpty(_productUsername) ? userLogin.LoginName : _productUsername;

                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);

                if (string.IsNullOrEmpty(company.CompanyInstanceSourceId))
                {
                    WriteToErrorLog($"ManageProductRum.ManageRumUser- Error for user with editorPersona id - {editorPersonaId} Error - Company not found.");
                    return "Company Setup Error: Please Contact Support.";
                }

                int companyId = Convert.ToInt32(company.CompanyInstanceSourceId);

                // super user
                if (IsSuperUser(userPersonaId))
                {
                    WriteToDiagnosticLog($"ManageProductRum.ManageRumUser - new user is Super user with editorPersona id - {editorPersonaId}.");
                    propertiesList.Add(companyId);
                    userAccessType = UserType.PortfolioManager.ToString();
                    var SysAdminRoleForRUM = _productInternalSettingList.FirstOrDefault(item => item.Name.Equals("UtilitySuperUser", StringComparison.OrdinalIgnoreCase)); 
                    userPropertyRegionRole.RoleList.Add(SysAdminRoleForRUM.Value);
                }
                else
                {
                    
                    if (userPropertyRegionRole.PropertyGroupList.Count > 0)
                    {
                        userAccessType = UserType.GroupManager.ToString();

                        foreach (var group in userPropertyRegionRole.PropertyGroupList)
                        {
                            propertiesList.Add(Convert.ToInt32(group));
                        }
                    }

                    if (userPropertyRegionRole.PropertyList.Count > 0)
                    {
						if (userPropertyRegionRole.PropertyList[0].ToUpper() != "ALL")
						{
							userAccessType = UserType.PropertyManager.ToString();

							foreach (var property in userPropertyRegionRole.PropertyList)
							{
								propertiesList.Add(Convert.ToInt32(property));
							}
						}
						else
						{
							propertiesList.Add(companyId);
							userAccessType = UserType.PortfolioManager.ToString();
						}
                    }
                }


                RumUser rumUser = null;

                rumUser = new RumUser
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Email = userEmailAddress,
                    Phone = "",
                    RealPageName = "GreenBook",
                    UserName = productLoginName,
                    UserTypeCode = userAccessType,
                    PortfolioId = companyId,
                    AssetIds = propertiesList,
                    Roles = userPropertyRegionRole.RoleList
                };
                Dictionary<string, object> logData = new Dictionary<string, object> { { "rumuser", rumUser } };
                WriteToDiagnosticLog($"ManageProductRum.ManageRumUser - Json to call product API for user with editorPersona id - {editorPersonaId}", logData);

                if (string.IsNullOrEmpty(_productUsername)) // NEW USER
                {
                    // check if user name exists in product
                    if (!string.IsNullOrEmpty(productLoginName))
                    {
                        bool foundNewUserName = false;
                        int incrementor = 0;
                        while (!foundNewUserName)
                        {
                            bool result = CheckUserExistsInRum(productLoginName);
                            if (result)
                            {
                                incrementor++;
                                productLoginName = productLoginName + incrementor.ToString();
                                WriteToDiagnosticLog($"User {productLoginName} already exists in On Site product with editorPersona id -{editorPersonaId}. Getting new one.");
                            }
                            else
                            {
                                foundNewUserName = true;
                            }
                        }

                        // reassign in case user name change
                        rumUser.UserName = productLoginName;
                    }

                    WriteToDiagnosticLog($"ManageProductRum.ManageOnSiteUse - trying to CREATE user with editorPersona id - {editorPersonaId}.");
                    var insertResult = InsertRumProductUser(userPersonaId, editorPersonaId, productLoginName, rumUser, companyId);

                    // add activity log
                    if (string.IsNullOrEmpty(insertResult))
                    {
                        // add activity log
                        WriteCreateUserActivityLog(editorPersonaId, person, userLogin);
                    }
                    return insertResult;
                }
				
				WriteToDiagnosticLog($"ManageProductRum.ManageRumUser - trying to UPDATE user with editorPersona id - {editorPersonaId}.");
				var updateResult = UpdateRumProductUser(userPersonaId, editorPersonaId, rumUser);

				if (string.IsNullOrEmpty(updateResult))
				{
					// add activity log
					WriteUpdateUserActivityLog(editorPersonaId, person, userLogin);
				}
				return updateResult;				
                
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"ManageProductRum.ManageRumUser - Error for user with editorPersona id - {editorPersonaId}", exception: ex);
                return $"Error - {ex.Message}";
            }
        }

        #endregion

        #region Migration
        /// <summary>
        /// List all users
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter datafilter)
        {
            var response = new ListResponse()
            {
                IsError = true,
                ErrorReason = "No Users."
            };
            var claimResposnse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResposnse.IsError) { response.ErrorReason = claimResposnse.ErrorReason; return response; }

            try
            {

                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
                if (companyInstanceSourceId == 0)
                {
                    WriteToErrorLog(
                        $"ManageProductRum.GetMigrationUsers.GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}.");
                    response.ErrorReason = "Company Setup Error: Please Contact Support.";
                    return response;
                }
                var filter = "NonMigrated";
                var startRow = 0;
                var resultPerRow = 1000;
                if (datafilter != null)
                {
                    if (datafilter.FilterBy.ContainsKey("filter"))
                    {
                        filter = datafilter.FilterBy["filter"];
                    }
                    if (datafilter.Pages != null)
                    {
                        startRow = datafilter.Pages.StartRow;
                        resultPerRow = datafilter.Pages.ResultsPerPage;
                    }
                }

                var url = $"{_apiEndPoint}/migration/{companyInstanceSourceId}/users?filter={filter}&startRow={startRow}&resultsPerPage={resultPerRow}";
                WriteToDiagnosticLog("ManageProductRum.GetMigrationUsers", new Dictionary<string, object> { { "Url", url } });

                var allUsers = GetResultFromApi<IList<MigrationUser>>(_accessToken, url);

                if (allUsers == null)
                {
                    WriteToErrorLog($"ManageProductRum.GetMigrationUsers-no users received from product for user with editorPersona id - {editorPersonaId}.");
                    return response;
                }
                WriteToDiagnosticLog($"ManageProductRum.GetUsers - Received users from product for user with editorPersona id - {editorPersonaId}.");
                response.RowsPerPage = resultPerRow;
                response.ErrorReason = string.Empty;
                response.IsError = false;
                response.TotalPages = 1;
                response.Records = allUsers.Cast<object>().ToList();
                response.TotalRows = allUsers.Count();
            }
            catch (Exception ex)
            {
                response = new ListResponse
                { 
                    IsError = true,
                    ErrorReason = ex.Message
                };
                WriteToErrorLog($"ManageProductRum.GetMigrationUsers Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
            }
            return response;
        }

        /// <summary>
        /// Update the user migration status
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="migrateUsers"></param>
        /// <returns></returns>
        public MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers)
        {
            var migrateResponse = new MigrateResponse()
            {
                Status = false,
                Message = "Could not migrate users."
            };
            var claimResposnse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResposnse.IsError) { migrateResponse.Message =  claimResposnse.ErrorReason; return migrateResponse; }

            try
            {

                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
                if (companyInstanceSourceId == 0)
                {
                    WriteToErrorLog(
                        $"ManageProductRum.UpdateUsersMigrationStatus.GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}.");
                    migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                    return migrateResponse;
                }

                var url = $"{_apiEndPoint}/migration/{companyInstanceSourceId}/migrate-users";

                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                var response = _client.PostAsJsonAsync($"{_apiEndPoint}/migration/{companyInstanceSourceId}/migrate-users", migrateUsers).Result;
                var responseContent = response.Content.ReadAsStringAsync().Result;

                var logData = new Dictionary<string, object>
            {
                { "Url", url },
                { "Response", responseContent },
                { "EditorPersonaId", editorPersonaId },
                { "MigratedUser", migrateUsers }
            };

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog("ManageProductRum.UpdateUsersMigrationStatus.PostAsJsonAsync", logData);
                    migrateResponse.Message = "Success";
                    migrateResponse.Status = true;
                }
                else
                {
                    WriteToErrorLog($"ManageProductRum.UpdateUsersMigrationStatus.PostAsJsonAsync", logData);
                    migrateResponse.Message = "Cannot update user status to migrated.";
                    migrateResponse.Status = false;
                }
            }
            catch (Exception ex )
            {
                migrateResponse = new MigrateResponse
                { 
                    Status = false,
                    Message = ex.Message
                };

                WriteToErrorLog($"ManageProductRum.UpdateUsersMigrationStatus Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
            }
            return migrateResponse;
        }

        #region User-Status

        /// <summary>
        /// Delete the Rum product user in Rum.
        /// </summary>
        /// <param name="editorPersonaId">The editor persona identifier.</param>
        /// <param name="productUserId">The product user identifier.</param>
        public bool ChangeUserStatus(long editorPersonaId, string productUserId)
        {
            var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (listResponse.IsError)
            {
                WriteToErrorLog(
                 $"ManageProductRum.ChangeUserStatus - Error for user with productUserId:{productUserId} and editorPersonaId:{editorPersonaId}. ErrorReason-{listResponse.ErrorReason}");
                return false;
            }

            _productUserId = productUserId;
            //De Activate User
            var result = DeleteRumUser(editorPersonaId, 0);

            if (string.IsNullOrEmpty(result))
            {
                WriteToDiagnosticLog($"ManageProductRum.ChangeUserStatus productUserId:{productUserId} and editorPersonaId:{editorPersonaId}");
                return true;
            }

            return false;
        }

        #endregion
        #endregion

        #region Private Methods
        /// <summary>
        /// Delete User
        /// </summary> 
        private string DeleteRumUser(long editorPersonaId, long userPersonaId)
		{
			string result = string.Empty;
			Dictionary<string, object> logData = new Dictionary<string, object>();		
			WriteToDiagnosticLog($"ManageProductRum.DeleteRumUser userPersonaId:{userPersonaId}");

			//UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);

			string baseUrlAndQuery = $"{_apiEndPoint}/user/deleteuser?userId={_productUserId}";
			logData.Add("uri", baseUrlAndQuery);

			using (var client = new HttpClient(_messageHandler, false))
			{
				client.DefaultRequestHeaders.Clear();
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
				var response = client.DeleteAsync(baseUrlAndQuery).Result;

				if (response.IsSuccessStatusCode)
				{
					return result;
				}
				else {
					logData = new Dictionary<string, object>();
					var erroMessage = response.Content.ReadAsStringAsync().Result.ToString();
					logData.Add("error", erroMessage);
					logData.Add("status", response.StatusCode);
					WriteToDiagnosticLog($"ManageProductRum.DeleteRumUser - Error for user with editorPersona id - {editorPersonaId}");
					WriteToDiagnosticLog($"ManageProductRum.DeleteRumUser - Error - {erroMessage}");
					return  $"There was a problem Delete Rum User the user with editorPersona id - {editorPersonaId} - Error-{erroMessage}.";
				}
			}
		}

		/// <summary>
		/// ReActivate User
		/// </summary> 
		private void ReActivateRumUser(long editorPersonaId, long userPersonaId)
		{
			Dictionary<string, object> logData = new Dictionary<string, object>();
			WriteToDiagnosticLog($"ManageProductRum.ReActivateRumUser userPersonaId:{userPersonaId}");

			string baseUrl = $"{_apiEndPoint}/user/reactivateuser?userId=" + _productUserId;
			logData.Add("uri", baseUrl);

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Clear();
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);				
				var response = client.PostAsJsonAsync(baseUrl,new { }).Result;

				if (response.IsSuccessStatusCode)
				{
					WriteReActivatedActivityLog(editorPersonaId, userPersonaId);
				}
				else
				{
					logData = new Dictionary<string, object>();
					var erroMessage = response.Content.ReadAsStringAsync().Result.ToString();
					logData.Add("error", erroMessage);
					logData.Add("status", response.StatusCode);
					WriteToDiagnosticLog($"ManageProductRum.ReActivateRumUser - Error for user with editorPersona id - {editorPersonaId}");
					WriteToDiagnosticLog($"ManageProductRum.ReActivateRumUser - Error - {erroMessage}");
				}
			}
		}

		private IList<RumPropertyGroup> GetRumPropertiesData(long companyInstanceSourceId, string type)
        {
            IList<RumPropertyGroup> propGroups = new List<RumPropertyGroup>();

            string baseUrlAndQuery = $"{_apiEndPoint}/identity/AccessItems?portfolioId={companyInstanceSourceId}&accessTypeCd={type}";
            var result = GetResultFromApi<IList<dynamic>>(_accessToken, baseUrlAndQuery, false);
            WriteToDiagnosticLog($"ManageProductRum.GetRumPropertiesData - Base Uri - {baseUrlAndQuery}");
            WriteToDiagnosticLog($"ManageProductRum.GetRumPropertiesData - result - {JsonConvert.SerializeObject(result)}");
            if (result != null)
            {
                foreach (var x in result)
                {
                    propGroups.Add(new RumPropertyGroup { Id = x.AccessId, Name = x.AccessName, IsAssigned = false });
                }
            }

            return propGroups;
        }

        private IList<Role> GetRumRoles(long companyInstanceSourceId)
        {
            IList<Role> roles = new List<Role>();

            string baseUrlAndQuery = $"{_apiEndPoint}/roleoptions/get?companyId={companyInstanceSourceId}";
            var result = GetResultFromApi<IList<dynamic>>(_accessToken, baseUrlAndQuery, false);

            WriteToDiagnosticLog($"ManageProductRum.GetRumRoles - Base Uri - {baseUrlAndQuery}");
            if (result != null)
            {
                foreach (var x in result.Select((x, i) => new { Item = x, Index = i }))
                {
                    if (!x.Item.InternalOnly.Value)
                    {
                        roles.Add(new Role { Id = x.Index + 101, Description = x.Item.RoleDescription.Value, Name = x.Item.RoleName.Value, IsAssigned = false });
                    }
                }
            }

            return roles;
        }

        private T GetResultFromApi<T>(string token, string baseUrlAndQuery, bool throwOnError = true) where T : class
        {
            T results = null;
            Dictionary<string, object> logData = new Dictionary<string, object>();
            logData.Add("uri", baseUrlAndQuery);
            using (var client = new HttpClient(_messageHandler, false))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync(baseUrlAndQuery).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    results = JsonConvert.DeserializeObject(jsonContent, typeof(T)) as T;
                }
                else
                {
                    //if (!(response.StatusCode == System.Net.HttpStatusCode.Unauthorized))
                    logData = new Dictionary<string, object>();
                    logData.Add("error", response.Content.ReadAsStringAsync().Result);
                    logData.Add("status", response.StatusCode);
                    WriteToDiagnosticLog("GetAsync - Exiting after error. ", logData);
                }
            }

            return results;
        }

        private RumUserClaims GetRumUserClaims(long userPersonaId)
        {
            //var userID = getRumProductUserFromGB(userPersonaId);
            string baseUrlAndQuery = $"{_apiEndPoint}/user/getuser?userId={_productUserId}";
            return GetResultFromApi<RumUserClaims>(_accessToken, baseUrlAndQuery, false);
        }

        private bool CheckUserExistsInRum(string productLoginName)
        {
            bool userExists = false;
            string baseUrlAndQuery = $"{_apiEndPoint}/user/userexists?userName={productLoginName}";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
                var response = client.GetAsync(baseUrlAndQuery).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    userExists = Convert.ToBoolean(JsonConvert.DeserializeObject(jsonContent));
                }
            }
            return userExists;
        }
        private void GetToken()
        {
            try
            {
                WriteToDiagnosticLog("ManageProductRum.GetToken - Begining of the method.");
                string nwpScope = "greenbooknwpapi";
                ObjectCache tokenCache = MemoryCache.Default;

                // Get token values from cache
                _accessToken = tokenCache["access_token_RUM"] as string;
                WriteToDiagnosticLog($"ManageProductRum.GetToken - Cached accessToken - {_accessToken}");

                if (string.IsNullOrEmpty(_accessToken))
                {
                    WriteToDiagnosticLog("ManageProductRum.GetToken - Null cache value. Getting new token.");

                    //var tokenUri = ConfigReader.GetIssuerUri;
                    
                    WriteToDiagnosticLog($"ManageProductRum.GetToken - GetTokenClient from IssueURI {_nwpIssueUri}.");

                    var tokenResponse = _tokenClient.RequestClientCredentialsAsync(nwpScope).Result;

                    if (tokenResponse.IsError)
                    {
                        throw new Exception($"ManageProductRum.GetToken - Received null or empty token. {tokenResponse.Error}");
                    }

                    var cachePolicy = new CacheItemPolicy
                    {
                        // Expier cache every after 9 minutes (assuming 10 min is token expiration time)
                        AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(9)
                    };

                    _accessToken = tokenResponse.AccessToken;

                    tokenCache.Set("access_token_RUM", _accessToken, cachePolicy);
                    Dictionary<string, object> logData = new Dictionary<string, object>() { { "accessToken", _accessToken } };
                    WriteToDiagnosticLog("ManageProductRum.GetToken - Got token, received & populated cache with token value.", logData);
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"Error in ManageProductRum.GetToken- {ex.Message}");
                throw new Exception($"Error in ManageProductRum.GetToken- {ex.Message}");
            }
        }

        private void CreateProductUserInGreenBook(long userPersonaId, dynamic userResult, string productLoginName, string userType)
        {
            string newid = Convert.ToString(userResult);

            WriteToDiagnosticLog($"ManageProductRum.CreateProductUserInGreenBook - Inserting in GB -productUsername -{productLoginName} and userId {newid}.");
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, productLoginName);
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newid);
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.NWPUserType, userType);

            WriteToDiagnosticLog("ManageProductRum.CreateProductUserInGreenBook - Create user Success. Set product status to Success");
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
        }

        private string InsertRumProductUser(long userPersonaId, long editorPersonaId, string productLoginName, RumUser rumUser, int companyId)
        {
            string result = string.Empty;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                WriteToDiagnosticLog($"ManageProductRum.InsertRumProductUser - calling product API for user with editorPersona id - {editorPersonaId}.");

                var response = client.PostAsJsonAsync($"{_apiEndPoint}/user/postuser", rumUser).Result;

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog($"ManageProductRum.InsertRumProductUser - IsSuccessStatusCode return true for user with editorPersona id - {editorPersonaId}.");
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    dynamic userResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                    if (userResult != null)
                    {
                        // for new user insert record
                        CreateProductUserInGreenBook(userPersonaId, userResult, productLoginName, rumUser.UserTypeCode);
                        result = string.Empty;
                    }
                }
                else
                {
                    string errorContent = string.Empty;
                    try
                    {
                        errorContent = response.Content.ReadAsStringAsync().Result;
                    }
                    catch
                    {/*Ignored*/
                    }

                    WriteToErrorLog(
                       $"ManageProductRum.InsertRumProductUser - Error for user with editorPersona id- {editorPersonaId} Error - {errorContent}.");
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                    result = $"There was a problem creating the user with editorPersona id - {editorPersonaId}. Error-{errorContent}";
                }
            }

            return result;
        }

        private string UpdateRumProductUser(long userPersonaId, long editorPersonaId, RumUser rumUser)
        {
            string result = string.Empty;
			//Check to see if user is inactive,if so re activate user before any update
			UpdateInactiveUser(editorPersonaId, userPersonaId);

			using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                WriteToDiagnosticLog($"ManageProductRum.UpdateOnSiteProductUser - calling product API for user with editorPersona id - {editorPersonaId}.");

                // var userID = getRumProductUserFromGB(userPersonaId);
                var response = client.PutAsJsonAsync($"{_apiEndPoint}/user/putuser?userId={_productUserId}", rumUser).Result;

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog($"ManageProductRum.UpdateOnSiteProductUser - IsSuccessStatusCode return true for user with editorPersona id - {editorPersonaId}.");

                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    dynamic userResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                    if (userResult != null)
                    {
                        result = string.Empty;
                    }
                    // Update saml settings in GB
                    UpdateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.NWPUserType, rumUser.UserTypeCode);
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
                }
                else
                {
                    string errorContent = string.Empty;
                    try
                    {
                        errorContent = response.Content.ReadAsStringAsync().Result;
                    }
                    catch
                    {/*Ignored*/ }
                    Dictionary<string, object> logData = new Dictionary<string, object>() { { "errorContent", errorContent } };
                    WriteToErrorLog(
                        $"ManageProductRum.UpdateOnSiteProductUser.UpdateOnSiteProductUser Error for user with editorPersona id - {editorPersonaId}.", logData);
                    result = $"There was a problem updating the user with editorPersona id - {editorPersonaId} - Error-{errorContent}.";
                }
            }

            return result;
        }

		private void UpdateInactiveUser(long editorPersonaId, long userPersonaId) 
		{
			RumUserClaims rumUser = GetRumUserClaims(userPersonaId);
			WriteToDiagnosticLog($"ManageProductRum.UpdateInactiveUser - calling product API for user with editorPersona id - {editorPersonaId}.");
			WriteToDiagnosticLog($"ManageProductRum.UpdateInactiveUser - user claims - {rumUser}.");
			
			if (rumUser != null)
			{
				// if a user record exists
				List<UserClaim> userClaims = (List<UserClaim>)rumUser.Claims;				
				var userCrmstatus = userClaims.Where(a => a.Type == "crmstatus").Select(b => b.Value).FirstOrDefault();
				WriteToDiagnosticLog($"ManageProductRum.UpdateInactiveUser - user current status - {userCrmstatus}.");
				if (userCrmstatus != null && userCrmstatus == "Inactive")
				{
					ReActivateRumUser(editorPersonaId, userPersonaId);
				}
			}
		}

		private ListResponse MergeRumPropertiesWithGreenbook(IList<RumPropertyGroup> allPropertyGroups, long userPersonaID)
        {

            RumUserClaims rumUser = GetRumUserClaims(userPersonaID);

            if (rumUser == null)
            {
                WriteToErrorLog($"Rum Services - MergeRumPropertiesWithGreenbook error for user {_productUserId} - User not found.");
                return new ListResponse() { IsError = true, ErrorReason = "User not found." };
            }

            // if a user record exists
            List<UserClaim> userClaims = (List<UserClaim>)rumUser.Claims;
            string type = "";
            var userAccessLevel = userClaims.Where(a => a.Type == "nwpusertype").Select(b => b.Value).FirstOrDefault();

            if (userAccessLevel == "RM")
            {
                type = "regionid";
                WriteToDiagnosticLog($"ManageProductRum.MergeRumPropertiesWithGreenbook accessType - regionalGroup");
            }
            else if (userAccessLevel == "GM")
            {
                type = "groupid";
                WriteToDiagnosticLog($"ManageProductRum.MergeRumPropertiesWithGreenbook accessType - propertyGroup");
            }
            else if (userAccessLevel == "PR")
            {
                type = "propid";
                WriteToDiagnosticLog($"ManageProductRum.MergeRumPropertiesWithGreenbook accessType - specificProperties");
            }
			else if (userAccessLevel == "PM")
			{
				type = "propid";
				WriteToDiagnosticLog($"ManageProductRum.MergeRumPropertiesWithGreenbook accessType - portfolio");
			}

			var propertyIds = (from a in userClaims
                               where a.Type == type
                               select a.Value);

            foreach (var property in propertyIds)
            {
                if (allPropertyGroups.Any(a => a.Id == Convert.ToInt32(property)))
                {
                    RumPropertyGroup rpg = (from a in allPropertyGroups
                                            where a.Id == Convert.ToInt32(property)
                                            select a).FirstOrDefault();

                    if (rpg != null)
                    {
                        rpg.IsAssigned = true;
                    }
                }
            }

            return new ListResponse()
            {
                Records = allPropertyGroups.Cast<object>().ToList(),
                TotalRows = allPropertyGroups.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1 
            };
        }

        private ListResponse MergeRumGlobalRolesWithGreenbook(IList<ProductRole> allRoles, long userPersonaID)
        {
            var accessType = new Dictionary<string, string>();

            RumUserClaims rumUser = GetRumUserClaims(userPersonaID);

            if (rumUser == null)
            {
                WriteToErrorLog($"Rum Services - MergeRumGlobalRolesWithGreenbook error for user {_productUserId} - User not found.");
                return new ListResponse() { IsError = true, ErrorReason = "User not found." };
            }

            // if a user record exists
            List<UserClaim> userClaims = (List<UserClaim>)rumUser.Claims;
            string type = "";
            var userAccessLevel = userClaims.Where(a => a.Type == "nwpusertype").Select(b => b.Value).FirstOrDefault();

            ProductRole rpg = (from a in allRoles
                                    where a.ID == userAccessLevel
                               select a).FirstOrDefault();

            if (rpg != null)
            {
                rpg.IsAssigned = true;
            }

            return new ListResponse()
            {
                Records = allRoles.Cast<object>().ToList(),
                TotalRows = allRoles.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1,
                Additional = accessType
            };
        }

        private ListResponse MergeUserRolesWithProductRoles(IList<Role> allRoles, long userPersonaID)
        {
            RumUserClaims rumUser = GetRumUserClaims(userPersonaID);

            if (rumUser == null)
            {
                WriteToErrorLog($"Rum Services - MergeRumPropertiesWithGreenbook error for user {_productUserId} - User not found.");
                return new ListResponse() { IsError = true, ErrorReason = "User not found." };
            }

            // if a user record exists
            List<UserClaim> userClaims = (List<UserClaim>)rumUser.Claims;

            var productRoles = (from a in userClaims
                                where a.Type == "role"
                                select a.Value);

            foreach (var productRole in productRoles)
            {
                if (allRoles.Any(a => a.Name == productRole))
                {
                    Role role = (from a in allRoles
                                 where a.Name == productRole
                                 select a).FirstOrDefault();

                    if (role != null)
                    {
                        role.IsAssigned = true;
                    }
                }
            }

            return new ListResponse()
            {
                Records = allRoles.Cast<object>().ToList(),
                TotalRows = allRoles.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1,
                Additional = string.Empty
            };
        }
        #endregion
    }	
}
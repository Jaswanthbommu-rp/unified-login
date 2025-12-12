using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using IdentityModel.Client;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.RPDocumentManagement;
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
        public ManageProductRum(DefaultUserClaim userClaims) : base((int)ProductEnum.UtilityManagement, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
#if DEBUG
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageProductRum", "Ctor - Getting Product settings." });
#endif
            _productId = (int)ProductEnum.UtilityManagement;
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _editorRealPageId = userClaims.UserRealPageGuid;
	        _userClaims = userClaims;
            _blueBook = new ManageBlueBook(userClaims);
            _apiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
            _apiSecret = _productInternalSettingList.First(a => a.Name.ToUpper() == "APISECRET").Value;
            _clientId = _productInternalSettingList.First(a => a.Name.ToUpper() == "CLIENTID").Value;
            _nwpIssueUri = _productInternalSettingList.First(a => a.Name.ToUpper() == "TOKENURL").Value;
#if DEBUG
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageProductRum", "Ctor - Received Product settings; getting token." });
#endif
            _tokenClient = new TokenClient($"{_nwpIssueUri}/connect/token", _clientId, _apiSecret);

            GetToken();
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="editorRealPageId"></param>
        /// <param name="userClaims"></param>
        /// <param name="messageHandler"></param>
        /// <param name="tokenMessageHandler"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="managePersona"></param>
        /// <param name="samlRepository"></param>
        /// <param name="blueBook"></param>
        /// <param name="productRepository"></param>
        /// <param name="repository"></param>
        public ManageProductRum(Guid editorRealPageId, DefaultUserClaim userClaims, HttpMessageHandler messageHandler, HttpMessageHandler tokenMessageHandler, 
            IProductInternalSettingRepository productInternalSettingRepository, IManagePersona managePersona, 
            ISamlRepository samlRepository, IManageBlueBook blueBook, IProductRepository productRepository, IRepository repository, HttpClient httpClient)
             : base((int)ProductEnum.UtilityManagement, userClaims, repository, tokenMessageHandler, httpClient)
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

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageProductRum", "Ctor - Received Product settings; getting token." });

            _tokenClient = new TokenClient($"{_nwpIssueUri}/connect/token", _clientId, _apiSecret, tokenMessageHandler);
            //_client = new HttpClient(messageHandler, false);
            //GetToken(); // not needed for unit tests
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Get Properties
        /// </summary>
        public ListResponse GetPropertyGroups(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor

                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);

                if (companyInstanceSourceId == 0)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
                    return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"GetProductCompanyInstanceId - Found blue book company instance source id - {companyInstanceSourceId}  for user editorPersona id -{editorPersonaId}" });


                // get access items from Rum product
                var groups = GetRumPropertiesData(companyInstanceSourceId, "GM");

                var allPropertyGroups = groups as IList<RumPropertyGroup> ?? groups.ToList();

                if (allPropertyGroups == null || allPropertyGroups.Count == 0)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"No properties received from product for user with editorPersona id - {editorPersonaId}." });

                    response.IsError = true;
                    response.ErrorReason = "No properties received from product.";
                    return response;
                }

                if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId)) // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"MergePropertiesWithGreenbook calling for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                    response = MergeRumPropertiesWithGreenbook(allPropertyGroups, userPersonaId);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"MergePropertiesWithGreenbook completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
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

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"Exiting method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId}." });
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

                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
            }

            return response;
        }

        /// <summary>
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
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Begining of method for user with editorPersona id - {editorPersonaId}" });

			try
			{
				result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO: need to refactor
				if (result.IsError)
				{
					WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
					return result;
				}

				string rumCompanyId = "";

				// get the PMCID from BlueBook because the user doesn't have the PMCID for Marketing Center yet
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", "Getting info from BlueBook.GetCompanyMap" });
                //IList<CompanyMap> companyMap = _blueBook.GetCompanyMap(_editorPersona.Organization.BooksMasterId, BlueBookProductConstants.UtilityManagement);
                IList<CustomerCompanyMap> companyMap = _blueBook.GetCompanyMap(_editorPersona.Organization.RealPageId, _editorPersona.Organization.BooksCustomerMasterId, source: BlueBookProductConstants.UtilityManagement, domain: _editorPersona.Organization.OrganizationDomain.Name);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", "Done getting info from BlueBook.GetCompanyMap" });
				if (companyMap != null && companyMap.Count > 0 && companyMap.Any(a => a.Source.ToUpper() == BlueBookProductConstants.UtilityManagement))
				{
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", "Getting PMC ID from BlueBook result" });
					rumCompanyId = companyMap.First(a => a.Source.ToUpper() == BlueBookProductConstants.UtilityManagement).CompanyInstanceSourceId;
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Found PMC ID from BlueBook result: {rumCompanyId}" });
				}

				var url = $"{ _apiEndPoint}/identity/Property?companyId= {rumCompanyId} ";
				logData = new Dictionary<string, object>();
				logData.Add("url", url);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", "Posting to url" }, logData: logData);

				var propertyList = GetResultFromApi<IList<ProductPropertyMap>>(_accessToken, url);

                if (propertyList != null && propertyList.Count > 0)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"GetPropertyInstance - Found total {propertyList.Count} properties with blue book company instance id {rumCompanyId} for user with editorPersona id - {editorPersonaId}." });

                    foreach (var property in propertyList)
                    {
                        RumPropertyGroup rpg = new RumPropertyGroup
                        {
                            Id = property.PropertyId,
                            Name = property.PropertyName,
                            State = property.State,
                            IsAssigned = false
                        };
                        rumProperties.Add(rpg);
                    }
                    // need to do a filter on the result
                    if (userPersonaId != 0 && (_productUserId != null && _productUserId.Length > 0)) // Called during updating Existing User
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Calling MergeProductPropertiesWithGreenbook for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                        result = MergeRumPropertiesWithGreenbook(rumProperties, userPersonaId);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"MergeProductPropertiesWithGreenbook completed for user with editorPersona id -{editorPersonaId}." });
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

                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"There was a problem getting the properties for user with editorPersona id - {editorPersonaId}." }, exception: ex);
			}

			return result;
		}

        /// <summary>
        /// Get Regions
        /// </summary>
        public ListResponse GetRegions(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRegions", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor

                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRegions", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
                if (companyInstanceSourceId == 0)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRegions", $"GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
                    return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };
                }

                // get access groups from on-site product
                var allRegions = GetRumPropertiesData(companyInstanceSourceId, "RM");

                if (allRegions == null)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRegions", $"No properties received from product for user with editorPersona id - {editorPersonaId}." });

                    response.IsError = true;
                    response.ErrorReason = CommonMessageConstants.RegionErrorMessage;
                    return response;
                }

                if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId)) // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRegions", $"MergeRegionsWithGreenbook calling for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                    response = MergeRumPropertiesWithGreenbook(allRegions, userPersonaId);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRegions", $"MergeRegionsWithGreenbook completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
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

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRegions", $"Exiting method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId}." });
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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRegions", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
            }

            return response;
        }

        /// <summary>
        ///Get roles
        /// </summary>
        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor

                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                //int companyInstanceSourceId = 279; // to get sample groups 
                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);

                // get roles from rum product
                var allRoles = GetRumRoles(companyInstanceSourceId);

                if (allRoles == null)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"No access groups (roles) received from product for user with editorPersona id - {editorPersonaId}." });

                    response.IsError = true;
                    response.ErrorReason = "No User Access groups (roles) received from product.";
                    return response;
                }

                if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId)) // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"MergeUserRolesWithProductRoles calling for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                    response = MergeUserRolesWithProductRoles(allRoles, userPersonaId);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"MergeUserRolesWithProductRoles completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
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

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Exiting method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId}." });
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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
            }

            return response;
        }

        public ListResponse GetUMGlobalRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUMGlobalRoles", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor

                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUMGlobalRoles", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                // get roles from rum product
                List<ProductRole> globalRoles = new List<ProductRole>();
                if (!_editorPersona.Organization.RealPageId.Equals(_contractCompanyRealPageId))
                {
                    //int companyInstanceSourceId = 279; // to get sample groups 
                    int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
                    if (companyInstanceSourceId == 0)
                    {
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUMGlobalRoles", $"GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
                        return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };
                    }

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
                }
                else
                {
                    globalRoles.Add(new ProductRole
                    {
                        ID = "SU",
                        Name = "Subcontractor",
                        Description = "Subcontractor",
                        IsAssigned = false
                    });
                }

                if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId)) // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUMGlobalRoles", $"MergeUserRolesWithProductRoles calling....for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                    response = MergeRumGlobalRolesWithGreenbook(globalRoles, userPersonaId);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUMGlobalRoles", $"MergeUserRolesWithProductRoles completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
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

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUMGlobalRoles", $"Exiting method with total rows - {response.TotalRows} for user with editorPersona id - {editorPersonaId}." });
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUMGlobalRoles", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignRumUser", $"Error for user with userPersonaId:{userPersonaId}. ErrorReason-{listResponse.ErrorReason}" });
                return listResponse.ErrorReason;
            }
            
            //De Activate User
            var result = DeleteRumUser(editorPersonaId, userPersonaId);            

            if (string.IsNullOrEmpty(result)) {
                //WriteDeActivatedActivityLog(editorPersonaId, userPersonaId); //commented this code to avoid double activity.
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignRumUser", $"UserPersonaId:{userPersonaId}" });
				UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);

			}			

            return result;
        }

		/// <summary>
		/// Update User Profile
		/// </summary>
		public string UpdateUserProfile(long editorPersonaId, long userPersonaId)
		{
			string result = string.Empty;
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUserProfile", $"Begin Update User Profile for user with editorPersona id - {editorPersonaId}." });
			try
			{
				var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
				if (listResponse.IsError)
				{
					WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUserProfile", $"Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}" });
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
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUserProfile", $"No email address for user with editorPersona id - {editorPersonaId}; assigning bogus email." });

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

					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUserProfile", $"Calling product API for user with editorPersona id - {editorPersonaId}." });

					var response = client.PutAsJsonAsync($"{_apiEndPoint}/user/putuserinfo?userId={_productUserId}", rumUser).Result;

					if (response.IsSuccessStatusCode)
					{
						WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUserProfile", $"IsSuccessStatusCode return true for user with editorPersona id - {editorPersonaId}." });

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
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUserProfile", $"Error for user with editorPersona id - {editorPersonaId}." }, logData: logData);
						result = $"There was a problem updating user profile for user with editorPersona id - {editorPersonaId} - Error-{errorContent}.";
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUserProfile", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
				return $"Error - {ex.Message}";
			}
		}

		/// <summary>
		/// Updated to create/update a user in On Site 
		/// </summary>
		public string ManageRumUser(long editorPersonaId, long userPersonaId, RumUserPropertyRegionRole userPropertyRegionRole, out List<AdditionalParameters> additionalParameters)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRumUser", $"Begin create/update user for user with editorPersona id - {editorPersonaId}." });
            additionalParameters = new List<AdditionalParameters>();
            try
            {
                if (userPropertyRegionRole == null)
                {
                    throw new Exception("RumUserPropertyRegionRole received null; check JSON in product batch table or parsing issue.");
                }

                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRumUser", $"Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}" });
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

                if (addresses != null && addresses.Any(a => a.AddressType.ToUpper() == "EMAIL"))
                {
                    userEmailAddress = (from a in addresses
                                        where
                                        a.AddressType.ToUpper() == "EMAIL"
                                        select a.AddressString).FirstOrDefault();
                }

                if (string.IsNullOrEmpty(userEmailAddress))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRumUser", $"No email address for user with editorPersona id - {editorPersonaId}; assigning bogus email." });

                    userEmailAddress = ValidateAndReturnEmailAddress(userLogin.LoginName);
                }

                var productLoginName = string.IsNullOrEmpty(_productUsername) ? userLogin.LoginName : _productUsername;
                int companyId = 0;
                string insUpdResult = string.Empty;
                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);

                var oldUserData = !string.IsNullOrEmpty(_productUsername) ? GetUserAccountableData(editorPersonaId, userPersonaId, "old") : new Dictionary<string, List<object>>();
                
                if (!_editorPersona.Organization.RealPageId.Equals(_contractCompanyRealPageId))
                {
                    
                    if (string.IsNullOrEmpty(company.CompanyInstanceSourceId))
                    {
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRumUser", $"Error for user with editorPersona id - {editorPersonaId} Error - Company not found." });
                        return "Company Setup Error: Please Contact Support.";
                    }

                    companyId = Convert.ToInt32(company.CompanyInstanceSourceId);

                    // super user
                    if (IsSuperUser(userPersonaId))
                    {
                        if (userPropertyRegionRole.RoleList.Count == 0)
                        {
                            RumUserClaims rumUserData = GetRumUserClaims(userPersonaId);

                            // if a user record exists
                            if (rumUserData != null)
                            {
                                List<UserClaim> userClaims = (List<UserClaim>)rumUserData.Claims;

                                var userproductRoles = (from a in userClaims
                                                        where a.Type == "role"
                                                        select a.Value);
                                var allRoles = GetRumRoles(companyId);

                                foreach (var userproductRole in userproductRoles)
                                {
                                    if (allRoles.Any(a => a.Name == userproductRole))
                                    {
                                        Role role = (from a in allRoles
                                                     where a.Name == userproductRole
                                                     select a).FirstOrDefault();

                                        if (role != null)
                                        {
                                            userPropertyRegionRole.RoleList.Add(role.Name);
                                        }
                                    }
                                }
                            }

                        }
						var SysAdminRoleForRUM = _productInternalSettingList.FirstOrDefault(item => item.Name.Equals("UtilitySuperUser", StringComparison.OrdinalIgnoreCase));
						userPropertyRegionRole.RoleList.Add(SysAdminRoleForRUM.Value);

						WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRumUser", $"New user is Super user with editorPersona id - {editorPersonaId}." });
						propertiesList.Add(companyId);
						userAccessType = UserType.PortfolioManager.ToString();
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
                }
                else
                {
                    if (userPropertyRegionRole.PropertyGroupList.Count > 0)
                    {
                        userAccessType = UserType.SubContractor.ToString();
                    }
                }

                RumUser rumUser = new RumUser
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
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRumUser", $"Json to call product API for user with editorPersona id - {editorPersonaId}" }, logData: logData);

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
                                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRumUser", $"User {productLoginName} already exists in On Site product with editorPersona id -{editorPersonaId}. Getting new one." });
                            }
                            else
                            {
                                foundNewUserName = true;
                            }
                        }

                        // reassign in case user name change
                        rumUser.UserName = productLoginName;
                    }

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRumUser", $"Trying to CREATE user with editorPersona id - {editorPersonaId}." });
                    insUpdResult = InsertRumProductUser(userPersonaId, editorPersonaId, productLoginName, rumUser, companyId);
                }
                else
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRumUser", $"Trying to UPDATE user with editorPersona id - {editorPersonaId}." });
                    insUpdResult = UpdateRumProductUser(userPersonaId, editorPersonaId, rumUser);
                }

                var newUserData = GetUserAccountableData(editorPersonaId, userPersonaId, "new");
                //Activity logs
                var activitylogs = GetActivityLogs(newUserData, oldUserData, rumUser);
                additionalParameters.AddRange(activitylogs);

                return insUpdResult;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRumUser", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
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
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
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
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", "Calling GetMigrationUsers endpoint" }, logData: new Dictionary<string, object> { { "Url", url } });

                var allUsers = GetResultFromApi<IList<MigrationUser>>(_accessToken, url);

                if (allUsers == null)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"No users received from product for user with editorPersona id - {editorPersonaId}." });
                    return response;
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Received users from product for user with editorPersona id - {editorPersonaId}." });
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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
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
            if (claimResposnse.IsError)
            {
                migrateResponse.Message = claimResposnse.ErrorReason;
                return migrateResponse;
            }

            try
            {

                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
                if (companyInstanceSourceId == 0)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
                    migrateResponse.Message = $"Company Setup Error: Please Contact Support. _udmSourceCode: {_udmSourceCode}";
                    return migrateResponse;
                }

                var url = $"{_apiEndPoint}/migration/{companyInstanceSourceId}/migrate-users";

                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                var response = _client.PostAsJsonAsync(url, migrateUsers).Result;
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
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", "PostAsJsonAsync Success" }, logData: logData);
                    migrateResponse.Message = "Success";
                    migrateResponse.Status = true;
                }
                else
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"PostAsJsonAsync Error" }, logData: logData);
                    migrateResponse.Message = "Cannot update user status to migrated.";
                    migrateResponse.Status = false;
                }
            }
            catch (Exception ex)
            {
                migrateResponse = new MigrateResponse
                {
                    Status = false,
                    Message = ex.Message
                };

                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
            }

            return migrateResponse;
        }

        #endregion

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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Error for user with productUserId:{productUserId} and editorPersonaId:{editorPersonaId}. ErrorReason-{listResponse.ErrorReason}" });
                return false;
            }

            _productUserId = productUserId;
            //De Activate User
            var result = DeleteRumUser(editorPersonaId, 0);

            if (string.IsNullOrEmpty(result))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"ProductUserId:{productUserId} and editorPersonaId:{editorPersonaId}" });
                return true;
            }

            return false;
        }
        #endregion

        #region Private Methods

        private List<AdditionalParameters> GetActivityLogs(Dictionary<string, List<object>> newUserData, Dictionary<string, List<object>> oldUserData, RumUser rumUser)
        {
            var additionalParameters = new List<AdditionalParameters>();
            //Roles
            var newRoles = newUserData.FirstOrDefault(x => x.Key == "newRoles").Value;
            var oldRoles = oldUserData.FirstOrDefault(x => x.Key == "oldRoles").Value;
            var currentRolesOnly = newRoles != null ? newRoles.Cast<Role>().Where(x => rumUser.Roles.Contains(x.Name.ToString())).ToList() : new List<Role>();
            var oldRolesOnly = oldRoles != null ? oldRoles.Cast<Role>().Where(x => x.IsAssigned).ToList() : new List<Role>();
            if (oldRolesOnly.Any())
            {
                foreach (var r in oldRolesOnly.Where(p => currentRolesOnly == null || !currentRolesOnly.Exists(c => c.Name == p.Name)))
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Utility Management Roles", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", r.Name) });
                }
            }
            if (currentRolesOnly.Any())
            {
                foreach (var r in currentRolesOnly.Where(p => oldRolesOnly == null || !oldRolesOnly.Exists(c => c.Name == p.Name)))
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Utility Management Roles", Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", r.Name) });
                }
            }

            //AccessType
            var newAccessType = newUserData.FirstOrDefault(x => x.Key == "newAccessType").Value;
            var oldAccessType = oldUserData.FirstOrDefault(x => x.Key == "oldAccessType").Value;
            var currentAccessTypeOnly = newAccessType != null ? newAccessType.Cast<ProductRole>().Where(x => rumUser.UserTypeCode == x.ID).ToList() : new List<ProductRole>();
            var oldAccessTypeOnly = oldAccessType != null ? oldAccessType.Cast<ProductRole>().Where(x => x.IsAssigned).ToList() : new List<ProductRole>();
            if (oldAccessTypeOnly.Any())
            {
                foreach (var r in oldAccessTypeOnly.Where(p => currentAccessTypeOnly == null || !currentAccessTypeOnly.Exists(c => c.Name == p.Name)))
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Utility Management Access Type", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", r.Name) });
                }
            }
            if (currentAccessTypeOnly.Any())
            {
                foreach (var r in currentAccessTypeOnly.Where(p => oldAccessTypeOnly == null || !oldAccessTypeOnly.Exists(c => c.Name == p.Name)))
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Utility Management Access Type", Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", r.Name) });
                }
            }

            //Properties
            var newProperties = newUserData.FirstOrDefault(x => x.Key == "newProperties").Value;
            var oldProperties = oldUserData.FirstOrDefault(x => x.Key == "oldProperties").Value;
            var currentPropertiesOnly = newProperties != null ? newProperties.Cast<RumPropertyGroup>().Where(x => rumUser.AssetIds != null && rumUser.AssetIds.Contains(Convert.ToInt32(x.Id))).ToList() : new List<RumPropertyGroup>();
            var oldPropertiesOnly = oldProperties != null ? oldProperties.Cast<RumPropertyGroup>().Where(x => x.IsAssigned).ToList() : new List<RumPropertyGroup>();
            if (oldPropertiesOnly.Any())
            {
                foreach (var r in oldPropertiesOnly)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Utility Management Properties", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", r.Name) });
                }
            }
            if (currentPropertiesOnly.Any())
            {
                foreach (var r in currentPropertiesOnly)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Utility Management Properties", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", r.Name) });
                }
            }

            //PropertyGroups
            var newPropertyGroups = newUserData.FirstOrDefault(x => x.Key == "newPropertyGroups").Value;
            var oldPropertyGroups = oldUserData.FirstOrDefault(x => x.Key == "oldPropertyGroups").Value;
            var currentPropertyGroupsOnly = newPropertyGroups != null ? newPropertyGroups.Cast<RumPropertyGroup>().Where(x => rumUser.AssetIds != null && rumUser.AssetIds.Contains(Convert.ToInt32(x.Id))).ToList() : new List<RumPropertyGroup>();
            var oldPropertyGroupsOnly = oldPropertyGroups != null ? oldPropertyGroups.Cast<RumPropertyGroup>().Where(x => x.IsAssigned).ToList() : new List<RumPropertyGroup>();
            if (oldPropertyGroupsOnly.Any())
            {
                foreach (var r in oldPropertyGroupsOnly)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Utility Management Property Group", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", r.Name) });
                }
            }
            if (currentPropertyGroupsOnly.Any())
            {
                foreach (var r in currentPropertyGroupsOnly)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Utility Management Property Group", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", r.Name) });
                }
            }

            return additionalParameters;
        }

        private Dictionary<string, List<object>> GetUserAccountableData(long editorPersonaId, long userPersonaId, string prefix)
        {
            Dictionary<string, List<object>> data = new Dictionary<string, List<object>>();
            var oldRolesResponse = GetRoles(editorPersonaId, userPersonaId, new RequestParameter());
            if (oldRolesResponse.Records != null)
            {
                var oldRoles = oldRolesResponse.Records.Cast<Role>().ToList();
                data.Add(prefix + "Roles", oldRoles.Cast<object>().ToList());
            }

            var oldPropertiesResponse = GetProperties(editorPersonaId, userPersonaId, new RequestParameter());
            if (oldPropertiesResponse.Records != null)
            {
                var oldProperties = oldPropertiesResponse.Records.Cast<RumPropertyGroup>().ToList();
                data.Add(prefix + "Properties", oldProperties.Cast<object>().ToList());
            }

            var oldPropertyGroupsResponse = GetPropertyGroups(editorPersonaId, userPersonaId, new RequestParameter());
            if (oldPropertyGroupsResponse.Records != null)
            {
                var oldPropertyGroups = oldPropertyGroupsResponse.Records.Cast<RumPropertyGroup>().ToList();
                data.Add(prefix + "PropertyGroups", oldPropertyGroups.Cast<object>().ToList());
            }

            var oldAccessTypeResponse = GetUMGlobalRoles(editorPersonaId, userPersonaId, new RequestParameter());
            if (oldAccessTypeResponse.Records != null)
            {
                var oldAccessType = oldAccessTypeResponse.Records.Cast<ProductRole>().ToList();
                data.Add(prefix + "AccessType", oldAccessType.Cast<object>().ToList());
            }
            return data;
        }

        /// <summary>
        /// Delete User
        /// </summary> 
        private string DeleteRumUser(long editorPersonaId, long userPersonaId)
		{
			string result = string.Empty;
			Dictionary<string, object> logData = new Dictionary<string, object>();		
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteRumUser", $"userPersonaId:{userPersonaId}" });

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
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteRumUser", $"Error for user with editorPersona id - {editorPersonaId} - Error - {erroMessage}" });
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
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ReActivateRumUser", $"userPersonaId:{userPersonaId}" });

			string baseUrl = $"{_apiEndPoint}/user/reactivateuser?userId=" + _productUserId;
			logData.Add("uri", baseUrl);

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Clear();
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);				
				var response = client.PostAsJsonAsync(baseUrl,new { }).Result;

				if (!response.IsSuccessStatusCode)
				{
					logData = new Dictionary<string, object>();
					var erroMessage = response.Content.ReadAsStringAsync().Result.ToString();
					logData.Add("error", erroMessage);
					logData.Add("status", response.StatusCode);
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ReActivateRumUser", $"Error for user with editorPersona id - {editorPersonaId} - Error - {erroMessage}" });
				}
			}
		}

		private IList<RumPropertyGroup> GetRumPropertiesData(long companyInstanceSourceId, string type)
        {
            IList<RumPropertyGroup> propGroups = new List<RumPropertyGroup>();

            string baseUrlAndQuery = $"{_apiEndPoint}/identity/AccessItems?portfolioId={companyInstanceSourceId}&accessTypeCd={type}";
            var result = GetResultFromApi<IList<dynamic>>(_accessToken, baseUrlAndQuery, false);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRumPropertiesData", $"Base Uri - {baseUrlAndQuery} - result - {JsonConvert.SerializeObject(result)}" });
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
            string baseUrlAndQuery = null;

            if (!_editorPersona.Organization.RealPageId.Equals(_contractCompanyRealPageId))
            {
                baseUrlAndQuery = $"{_apiEndPoint}/roleoptions/get?companyId={companyInstanceSourceId}";
            }
            else
            {
                baseUrlAndQuery = $"{_apiEndPoint}/roleoptions/GetRolesForType?userType=su";
            }
            var result = GetResultFromApi<IList<dynamic>>(_accessToken, baseUrlAndQuery, false);

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRumRoles", $"Base Uri - {baseUrlAndQuery}" });
            if (result != null)
            {
                foreach (var x in result.Select((x, i) => new { Item = x, Index = i }))
                {
                    if (!x.Item.InternalOnly.Value)
                    {
                        roles.Add(new Role { Id = x.Index + 101, Description = x.Item.RoleDescription.Value, Name = x.Item.RoleName.Value, IsAssigned = false });
                    }
                    else
                    {
                        roles.Add(new Role { Id = x.Item.RoleId, Description = x.Item.RoleDescription.Value, Name = x.Item.RoleName.Value, IsAssigned = false });
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
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetResultFromApi", "Exiting after error" }, logData: logData);
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
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetToken", "Begining of the method." });
                string nwpScope = "greenbooknwpapi";
                ObjectCache tokenCache = MemoryCache.Default;

                // Get token values from cache
                _accessToken = tokenCache["access_token_RUM"] as string;
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetToken", $"Cached accessToken - {_accessToken}" });

                if (string.IsNullOrEmpty(_accessToken))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetToken", "Null cache value. Getting new token." });

                    //var tokenUri = ConfigReader.GetIssuerUri;
                    
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetToken", $"GetTokenClient from IssueURI {_nwpIssueUri}." });

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
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetToken", "Got token, received & populated cache with token value." }, logData: logData);
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetToken", $"Error in - {ex.Message}" });
                throw new Exception($"Error in ManageProductRum.GetToken- {ex.Message}");
            }
        }

        private void CreateProductUserInGreenBook(long userPersonaId, dynamic userResult, string productLoginName, string userType)
        {
            string newid = Convert.ToString(userResult);

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateProductUserInGreenBook", $"Inserting in GB - productUsername - {productLoginName} and userId {newid}." });
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, productLoginName);
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newid);
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.NWPUserType, userType);

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateProductUserInGreenBook", "Create user Success. Set product status to Success" });
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

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "InsertRumProductUser", $"Calling product API for user with editorPersona id - {editorPersonaId}." });

                var response = client.PostAsJsonAsync($"{_apiEndPoint}/user/postuser", rumUser).Result;

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "InsertRumProductUser", $"IsSuccessStatusCode return true for user with editorPersona id - {editorPersonaId}." });
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

                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "InsertRumProductUser", $"Error for user with editorPersona id- {editorPersonaId} Error - {errorContent}." });
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

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRumProductUser", $"Calling product API for user with editorPersona id - {editorPersonaId}." });

                // var userID = getRumProductUserFromGB(userPersonaId);
                var response = client.PutAsJsonAsync($"{_apiEndPoint}/user/putuser?userId={_productUserId}", rumUser).Result;

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRumProductUser", $"IsSuccessStatusCode return true for user with editorPersona id - {editorPersonaId}." });

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
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRumProductUser", $"Error for user with editorPersona id - {editorPersonaId}." }, logData: logData);
                    result = $"There was a problem updating the user with editorPersona id - {editorPersonaId} - Error-{errorContent}.";
                }
            }

            return result;
        }

		private void UpdateInactiveUser(long editorPersonaId, long userPersonaId) 
		{
			RumUserClaims rumUser = GetRumUserClaims(userPersonaId);
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateInactiveUser", $"Calling product API for user with editorPersona id - {editorPersonaId} - user claims - {rumUser}." });
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateInactiveUser", $"user claims - {rumUser}." });
			
			if (rumUser != null)
			{
				// if a user record exists
				List<UserClaim> userClaims = (List<UserClaim>)rumUser.Claims;				
				var userCrmstatus = userClaims.Where(a => a.Type == "crmstatus").Select(b => b.Value).FirstOrDefault();
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateInactiveUser", $"User current status - {userCrmstatus}." });
				if (userCrmstatus != null && userCrmstatus == "Inactive")
				{
					ReActivateRumUser(editorPersonaId, userPersonaId);
				}
			}
		}

		private ListResponse MergeRumPropertiesWithGreenbook(IList<RumPropertyGroup> allPropertyGroups, long userPersonaID)
        {

            RumUserClaims rumUser = GetRumUserClaims(userPersonaID);

            var accessType = new Dictionary<string, string>();

            if (rumUser == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "MergeRumPropertiesWithGreenbook", $"Error for user {_productUserId} - User not found." });
                return new ListResponse() { IsError = true, ErrorReason = "User not found." };
            }

            // if a user record exists
            List<UserClaim> userClaims = (List<UserClaim>)rumUser.Claims;
            string type = "";
            var userAccessLevel = userClaims.Where(a => a.Type == "nwpusertype").Select(b => b.Value).FirstOrDefault();

            if (userAccessLevel == "RM")
            {
                type = "regionid";
                accessType.Add("accessType", "regionalGroup");
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeRumPropertiesWithGreenbook", "accessType - regionalGroup" });
            }
            else if (userAccessLevel == "GM")
            {
                type = "groupid";
                accessType.Add("accessType", "propertyGroup");
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeRumPropertiesWithGreenbook", "accessType - propertyGroup" });
            }
            else if (userAccessLevel == "PR")
            {
                type = "propid";
                accessType.Add("accessType", "specificProperties");
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeRumPropertiesWithGreenbook", $"accessType - specificProperties" });
            }
			else if (userAccessLevel == "PM")
			{
				type = "propid";
                accessType.Add("accessType", "allProperties");
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeRumPropertiesWithGreenbook", $"accessType - portfolio" });
			}

			var propertyIds = (from a in userClaims
                               where a.Type == type
                               select a.Value);

            foreach (var property in propertyIds)
            {
                if (allPropertyGroups.Any(a => a.Id == property))
                {
                    RumPropertyGroup rpg = (from a in allPropertyGroups
                                            where a.Id == property
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
                TotalPages = 1,
                Additional = accessType
            };
        }

        private ListResponse MergeRumGlobalRolesWithGreenbook(IList<ProductRole> allRoles, long userPersonaID)
        {
            var accessType = new Dictionary<string, string>();

            RumUserClaims rumUser = GetRumUserClaims(userPersonaID);

            if (rumUser == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "MergeRumGlobalRolesWithGreenbook", $"Error for user {_productUserId} - User not found." });
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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "MergeUserRolesWithProductRoles", $"Error for user {_productUserId} - User not found." });
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
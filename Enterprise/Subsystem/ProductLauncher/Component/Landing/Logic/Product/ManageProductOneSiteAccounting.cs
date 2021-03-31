using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Accounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSiteAccounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using blueBook = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using IC = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
	/// <summary>
	/// 
	/// </summary>
	public class ManageProductOneSiteAccounting : ManageProductBase, IManageProductOneSiteAccounting
	{
		private string _username;
		private string _password;
		private string _intactLogin;
		private string _intactPassword;

		private string _companyName;
        private DefaultUserClaim _userClaims;

        // Services
        private IOneSiteAccountingProductService _service = new OneSiteAccountingProductService();

        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="editorRealPageId">The RealPageId of the editor</param>
        /// <param name="userClaims"> User Claims</param>
        public ManageProductOneSiteAccounting(DefaultUserClaim userClaims) : base((int)ProductEnum.FinancialSuite, userClaims, null, null)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			_productId = (int)ProductEnum.FinancialSuite;

			_editorRealPageId = userClaims.UserRealPageGuid;
			_blueBook = new Logic.ManageBlueBook(userClaims);

			_productUrl = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
			_username = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.ToUpper() == "APIUSERNAME").Value));
			_password = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.ToUpper() == "APIPASSWORD").Value));
			_intactLogin = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.ToUpper() == "INTACTUSER").Value));
			_intactPassword = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.ToUpper() == "INTACTPASSWORD").Value));

			_service.Url = _productUrl;
			_service.PreAuthenticate = true;
			_service.Credentials = new System.Net.NetworkCredential(_username, _password);
			_userClaims = userClaims;
        }

		/// <summary>
		/// Unit test constructor
		/// </summary>
		/// <param name="editorRealPageId"></param>
		/// <param name="userClaims"></param>
		/// <param name="service"></param>
		/// <param name="samlRepository"></param>
		/// <param name="managePersona"></param>
		/// <param name="manageBlueBook"></param>
		/// <param name="productRepository"></param>
		/// <param name="productInternalSettingRepository"></param>
		/// <param name="managePartyRelationship"></param>
		public ManageProductOneSiteAccounting(Guid editorRealPageId, DefaultUserClaim userClaims, IOneSiteAccountingProductService service, ISamlRepository samlRepository, IManagePersona managePersona, IManageBlueBook manageBlueBook, IProductRepository productRepository, IProductInternalSettingRepository productInternalSettingRepository, IManagePartyRelationship managePartyRelationship) : base((int)ProductEnum.FinancialSuite, productInternalSettingRepository, productRepository)
		{
			_editorRealPageId = editorRealPageId;
			_service = service;
			_samlRepository = samlRepository;
			_managePersona = managePersona;
			_blueBook = manageBlueBook;
			_productRepository = productRepository;
			_productInternalSettingRepository = productInternalSettingRepository;
			_managePartyRelationship = managePartyRelationship;
			_userClaims = userClaims;
        }

		/// <summary>
		/// Unit test constructor
		/// </summary>
		/// <param name="editorRealPageId"></param>
		/// <param name="userClaims"></param>
		/// <param name="service"></param>
		/// <param name="samlRepository"></param>
		/// <param name="managePersona"></param>
		/// <param name="manageBlueBook"></param>
		/// <param name="productRepository"></param>
		/// <param name="productInternalSettingRepository"></param>
		/// <param name="manageElectronicAddress"></param>
		/// <param name="managePerson"></param>
		/// <param name="manageUserLogin"></param>
		/// <param name="managePartyRelationship"></param>
		public ManageProductOneSiteAccounting(Guid editorRealPageId, DefaultUserClaim userClaims, IOneSiteAccountingProductService service, ISamlRepository samlRepository, IManagePersona managePersona, IManageBlueBook manageBlueBook, IProductRepository productRepository, IProductInternalSettingRepository productInternalSettingRepository, IManageElectronicAddress manageElectronicAddress, IManagePerson managePerson, IManageUserLogin manageUserLogin, IManagePartyRelationship managePartyRelationship) : base((int)ProductEnum.FinancialSuite, productInternalSettingRepository, productRepository)
		{
			_editorRealPageId = editorRealPageId;
			_service = service;
			_samlRepository = samlRepository;
			_managePersona = managePersona;
			_blueBook = manageBlueBook;
			_productRepository = productRepository;
			_productInternalSettingRepository = productInternalSettingRepository;
			_manageElectronicAddress = manageElectronicAddress;
			_managePerson = managePerson;
			_manageUserLogin = manageUserLogin;
			_managePartyRelationship = managePartyRelationship;
            _userClaims = userClaims;
        }

		#region Property

		//
		/// <summary>
		/// Get the properties for the given user persona
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		public ListResponse GetUserProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
		{
			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (response.IsError) { return response; }
			FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);

			Property[] prop = new Property[1] { new Property() };
			List<NameValuePair> loginInfo = new List<NameValuePair>
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword }
			};
			if (!String.IsNullOrEmpty(_productUserId))
			{
				loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
			}
			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"GetUserProperties - _productUserId = {_productUserId}", logData);
			prop[0].NameValuePair = loginInfo.ToArray();

			TotalRows[] results2 = new TotalRows[1];
			LocationID[] location;
			IList<ProductProperty> list;

			try
			{   

                location = _service.GetAllProperties(prop, wsParams, out results2);
				logData = new Dictionary<string, object>();
				logData.Add("location", location);
				WriteToDiagnosticLog($"GetUserProperties - result from api", logData);
				list = location.ToGBProperties();

				if (list == null)
				{
					if (list == null)
					{
						if (results2.Length > 0)
						{
							string message = results2[0].TotalRows1;
							if (message.ToUpper().Contains("NOT A VALID USERID"))
							{
								throw new Exception("Invalid user");
							}
						}
						list = new List<ProductProperty>();
					}
				}

				Dictionary<string, bool> allProperties = new Dictionary<string, bool>();
				if (list.Any(a => a.IsAssigned == true))
				{
					allProperties.Add("allProperties", false);
				}
				else
				{
					allProperties.Add("allProperties", true);
				}
				response = new ListResponse()
				{
					Records = list.Cast<object>().ToList(),
					TotalRows = list.Count,
					RowsPerPage = list.Count,
					TotalPages = 1,
					ErrorReason = "",
					Additional = allProperties
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"GetUserProperties - Error", exception: ex);
				response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}
			return response;
		}

		//
		/// <summary>
		/// Get the property Groups for the given user persona
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		public ListResponse GetUserPropertyGroups(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
		{
			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (response.IsError) { return response; }
			FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);

			Property[] prop = new Property[1] { new Property() };
			List<NameValuePair> loginInfo = new List<NameValuePair>
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword }
			};
			if (!String.IsNullOrEmpty(_productUserId))
			{
				loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
			}
			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"GetUserPropertyGroups - _productUserId = {_productUserId}", logData);
			prop[0].NameValuePair = loginInfo.ToArray();

			TotalRows[] results2 = new TotalRows[1];
			LocationGroupID[] location;
			IList<ProductPropertyGroup> list;

			try
			{

				location = _service.GetAllPropertyGroups(prop, wsParams, out results2);
				logData = new Dictionary<string, object>();
				logData.Add("location", location);
				WriteToDiagnosticLog($"GetUserPropertyGroups - result from api", logData);
				list = location.ToGBPropertyGroup();

				if (list == null)
				{
					if (results2.Length > 0)
					{
						string message = results2[0].TotalRows1;
						if (message.ToUpper().Contains("NOT A VALID USERID"))
						{
							throw new Exception("Invalid user");
						}
					}
					list = new List<ProductPropertyGroup>();
				}

				response = new ListResponse()
				{
					Records = list.Cast<object>().ToList(),
					TotalRows = list.Count,
					RowsPerPage = list.Count,
					TotalPages = 1,
					ErrorReason = "",
					Additional = null
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"GetUserPropertyGroups - Error", exception: ex);
				response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}
			return response;
		}

		private ListResponse GetPropertyGroupEntities(List<ProductPropertyGroup> locationGroups, RequestParameter datafilter)
		{
			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			List<string> locationGrps = new List<string>();

			FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);

			locationGrps = locationGroups.Select(a => a.ID).ToList();

			Property[] prop = new Property[1] { new Property() };
			List<NameValuePair> loginInfo = new List<NameValuePair>
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword }
			};
			if (!String.IsNullOrEmpty(_productUserId))
			{
				loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
			}
			if (locationGrps.Count > 0)
			{
				loginInfo.Add(new NameValuePair { Name = "locGroupIds", Value = String.Join(",", locationGrps) });
			}
			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"GetPropertyGroupEntities - _productUserId = {_productUserId}", logData);
			prop[0].NameValuePair = loginInfo.ToArray();

			TotalRows[] results2 = new TotalRows[1];
			LocationGroupID[] location;
			IList<ProductPropertyGroup> list;

			try
			{

				location = _service.GetAllPropertyGroupMembers(prop, wsParams, out results2);
				logData = new Dictionary<string, object>();
				logData.Add("location", location);
				WriteToDiagnosticLog($"GetPropertyGroupEntities - result from api", logData);
				list = location.ToGBPropertyGroup();

				if (list == null)
				{
					if (results2.Length > 0)
					{
						string message = results2[0].TotalRows1;
						if (message.ToUpper().Contains("NOT A VALID USERID"))
						{
							throw new Exception("Invalid user");
						}
					}
					list = new List<ProductPropertyGroup>();
				}

				response = new ListResponse()
				{
					Records = list.Cast<object>().ToList(),
					TotalRows = list.Count,
					RowsPerPage = list.Count,
					TotalPages = 1,
					ErrorReason = "",
					Additional = null
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"GetPropertyGroupEntities - Error", exception: ex);
				response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}
			return response;
		}
		//
		/// <summary>
		/// Get the property Groups for the given user persona
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		public IList<ProductPropertyGroup> GetAllPropertyGroups(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
		{
			Dictionary<string, object> logData = new Dictionary<string, object>();
			FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);

			Property[] prop = new Property[1] { new Property() };
			List<NameValuePair> loginInfo = new List<NameValuePair>
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword }
			};
			if (!String.IsNullOrEmpty(_productUserId))
			{
				loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
			}
			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"GetAllPropertyGroups - _productUserId = {_productUserId}", logData);
			prop[0].NameValuePair = loginInfo.ToArray();

			TotalRows[] results2 = new TotalRows[1];
			LocationGroupID[] location;
			IList<ProductPropertyGroup> list;

			try
			{

				location = _service.GetAllPropertyGroups(prop, wsParams, out results2);
				logData = new Dictionary<string, object>();
				logData.Add("location", location);
				WriteToDiagnosticLog($"GetAllPropertyGroups - result from api", logData);
				list = location.ToGBPropertyGroup();

				if (list == null)
				{
					list = new List<ProductPropertyGroup>();
					WriteToDiagnosticLog($"GetAllPropertyGroups - returned null data - no error ", logData);
				}
			}
			catch (Exception ex)
			{
				list = null;
				WriteToErrorLog($"GetAllPropertyGroups - api Error", exception: ex);
			}
			return list;
		}
		//
		/// <summary>
		/// Get the property Groups for the given user persona
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		public ListResponse GetPropertyGroupEntities(long editorPersonaId, long userPersonaId,string locationGrpId, RequestParameter datafilter)
		{
			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (response.IsError) { return response; }
			FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);

			Property[] prop = new Property[1] { new Property() };
			List<NameValuePair> loginInfo = new List<NameValuePair>
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword }
			};
			if (!String.IsNullOrEmpty(_productUserId))
			{
				loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
			}
			if (!String.IsNullOrEmpty(locationGrpId))
			{
				loginInfo.Add(new NameValuePair { Name = "locGroupIds", Value = locationGrpId });
			}
			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"GetPropertyGroupEntities - _productUserId = {_productUserId}", logData);
			prop[0].NameValuePair = loginInfo.ToArray();

			TotalRows[] results2 = new TotalRows[1];
			LocationGroupID[] location;
			IList<ProductPropertyGroup> list;

			try
			{

				location = _service.GetAllPropertyGroupMembers(prop, wsParams, out results2);
				logData = new Dictionary<string, object>();
				logData.Add("location", location);
				WriteToDiagnosticLog($"GetPropertyGroupEntities - result from api", logData);
				list = location.ToGBPropertyGroup();

				if (list == null)
				{
					if (results2.Length > 0)
					{
						string message = results2[0].TotalRows1;
						if (message.ToUpper().Contains("NOT A VALID USERID"))
						{
							throw new Exception("Invalid user");
						}
					}
					list = new List<ProductPropertyGroup>();
				}

				response = new ListResponse()
				{
					Records = list.Cast<object>().ToList(),
					TotalRows = list.Count,
					RowsPerPage = list.Count,
					TotalPages = 1,
					ErrorReason = "",
					Additional = null
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"GetPropertyGroupEntities - Error", exception: ex);
				response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}
			return response;
		}

		//
		/// <summary>
		/// Get the properties for the given user persona
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		public ListResponse GetUserPropertiesNew(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            Dictionary<string, object> logData = new Dictionary<string, object>();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response; }
            
            try
            {
                WriteToDiagnosticLog($"GetUserPropertiesNew - GetAllCompanyProperties - _productUserId = {_productUserId} - START");

                List<ACProperty> companyPropertiesList = GetAllCompanyProperties(editorPersonaId, userPersonaId, datafilter);
                companyPropertiesList = companyPropertiesList.FindAll(m => m.PropertyId != string.Empty && m.PropertyName != string.Empty);

                WriteToDiagnosticLog($"GetUserPropertiesNew - GetAllCompanyProperties - _productUserId = {_productUserId} - END");

                List<ACCompany> cmpList = GetUserCompaniesDetails(editorPersonaId, userPersonaId, datafilter);

                //foreach (var comp in cmpList)
                //{
                //    if(comp.isAssigned == true)
                //    {
                //        companyPropertiesList.ForEach(m =>
                //        {
                //            if (m.CompanyId == comp.Id)
                //            {
                //                m.DisableSelection = true;
                //                m.IsAssigned = true;
                //            }
                //        });
                //    }
                //}

				if (companyPropertiesList.Count(p => !string.IsNullOrEmpty(p.MConsoleId.Trim())) != 0)
				{
					//We have MConsole company here
					companyPropertiesList.ForEach(x => x.Id = string.Concat(x.Id + "|" + x.CompanyId));
				}

                response = new ListResponse()
                {
                    Records = companyPropertiesList.Cast<object>().ToList(),
                    TotalRows = companyPropertiesList.Count,
                    RowsPerPage = companyPropertiesList.Count,
                    TotalPages = 1,
                    ErrorReason = "",
                    Additional = null
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"GetUserPropertiesNew - Error", exception: ex);
				//UI calls GetProperty but sometimes it's diplaying the data in Entities tab, that's why this message should be Entity instead of Property
				response = new ListResponse()
				{
					IsError = true,
					ErrorReason = CommonMessageConstants.EntityErrorMessage
                };
            }
            return response;
        }


        //
        /// <summary>
        /// Get the user details for the given user persona
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public NameValuePair[] GetUser(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
           
            Dictionary<string, object> logData = new Dictionary<string, object>();
           
            FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);
            
            SharedObjects.Product.OneSiteAccounting.User[] user = new SharedObjects.Product.OneSiteAccounting.User[1] { new SharedObjects.Product.OneSiteAccounting.User() };
            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName},
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };
            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }
            logData = new Dictionary<string, object>();
            logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
            WriteToDiagnosticLog($"GetUser - _productUserId = {_productUserId}", logData);
            user[0].NameValuePair = loginInfo.ToArray();

            
            NameValuePair[] userResp = null;
            IList<ProductProperty> list;

            try
            {

                userResp = _service.GetUser(user);

                logData = new Dictionary<string, object>();
                logData.Add("user details", userResp);
                WriteToDiagnosticLog($"GetUser - result from api", logData);
                
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"GetUserProperties - Error", exception: ex);
               
            }
            return userResp;
        }

      

        //
        /// <summary>
        /// Get the properties for the given user persona
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetUserCompanies(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            Dictionary<string, object> logData = new Dictionary<string, object>();

            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response; }

            FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);

            
            List<ACCompany> cmpList;
            AccountingUser aUser = new AccountingUser();

            try
            {                

                cmpList = GetUserCompaniesDetails(editorPersonaId, userPersonaId, datafilter);

                if (userPersonaId != 0)
                {
                    //List<ACProperty> companyPropertiesList = GetAllCompanyProperties(editorPersonaId, userPersonaId, datafilter);
                    //cmpList.ForEach(cmp =>
                    //{

                    //    List<ACProperty> props = companyPropertiesList.FindAll(p => p.CompanyId.ToUpperInvariant() == cmp.Id.ToUpperInvariant());
                    //    if (props.Count > 0)
                    //    {
                    //        if (props.Any(f => f.IsAssigned == false))
                    //        {
                    //            cmp.isAssigned = false;
                    //        }
                    //        else
                    //        {
                    //            cmp.isAssigned = true;
                    //        }
                    //    }

                    //});
                }

                NameValuePair[] userResp = null;

                List<int> prdIds = GetProductIdsByOrg();
                if(prdIds != null)
                {
                    if (prdIds.Contains((int)ProductEnum.SiteSpendManagement))
                    {
                        aUser.IsSiteSpendManagementAssignedToCompany = true;
                    }
                }
                

                //Get User details
                if (userPersonaId != 0)
                {
                    // Get User Data
                    userResp = GetUser(editorPersonaId, userPersonaId, datafilter);
                    if (userResp != null)
                    {
                        foreach (var item in userResp)
                        {
                            if (item.Name.ToUpperInvariant() == "UNRESTRICTED") 
                            {
                                aUser.HasAccessToAllCurrentFutureProperties = item.Value == "true" ? true : false;
                            }
                            if (item.Name.ToUpperInvariant() == "RPPORTALUSER")
                            {
                                aUser.HasAccessToSiteSpendManagementOnly = item.Value == "true" ? true : false;
                            }
                            if (item.Name.ToUpperInvariant() == "ADMIN")
                            {
                                aUser.IsAccountingAdmin = item.Value == "true" ? true : false;
                            }
                        }
                    }

                    aUser.HasAccessToAllCurrentFutureProperties = ComputeFlagBasedOnCompanyAndPropertySelected(editorPersonaId, userPersonaId, datafilter);                    
                }

                ListResponse propertyList = GetUserPropertiesNew(editorPersonaId, userPersonaId, datafilter);
                aUser.IsMConsolePMC = (propertyList.Records.Count(p => ((ACProperty)p).MConsoleId.Trim() != string.Empty) > 0) ? true : false;

                if (userResp == null) { userResp = new NameValuePair[1]; }
                
                response = new ListResponse()
                {
                    Records = cmpList.Cast<object>().ToList(),
                    TotalRows = cmpList.Count,
                    RowsPerPage = cmpList.Count,
                    TotalPages = 1,
                    ErrorReason = "",
                    Additional = aUser
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"GetUserProperties - Error", exception: ex);
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
					//UI calls GetPropertyGroup but it's dispalying the data in Companies Tab, so that's why the message should be "Companies"
					response.ErrorReason = CommonMessageConstants.CompanyTabErrorMessage;
				}
            }

            return response;
        }

        private bool ComputeFlagBasedOnCompanyAndPropertySelected(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            bool hasAccessToAllCurrentAndFutureProperties = false;
            List<ACCompany> companyList = GetUserCompaniesDetails(editorPersonaId, userPersonaId, datafilter);
            ListResponse propertyList = GetUserPropertiesNew(editorPersonaId, userPersonaId, datafilter);
            int totalCompanies = 0;
            int totalProperties = 0;
            int totalCompaniesSelected = 0;
            int totalPropertiesUnSelected = 0;

            totalCompanies = companyList.Count;
            totalCompaniesSelected = companyList.Count(c => c.isAssigned == true);

            totalProperties = propertyList.Records.Count;
            totalPropertiesUnSelected = propertyList.Records.Count(p => ((ACProperty)p).IsAssigned == false);

            if ((totalCompanies == totalCompaniesSelected) && (totalProperties == totalPropertiesUnSelected))
                hasAccessToAllCurrentAndFutureProperties = true;

            return hasAccessToAllCurrentAndFutureProperties;
        }

        private List<ACCompany> GetUserCompaniesDetails(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
          
            Dictionary<string, object> logData = new Dictionary<string, object>();

            Company[] comp = new Company[1] { new Company() };
            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName},
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };

            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }

            logData = new Dictionary<string, object>();
            logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
            WriteToDiagnosticLog($"GetUserCompaniesDetails - _productUserId = {_productUserId}", logData);
            comp[0].NameValuePair = loginInfo.ToArray();

            TotalRows[] results2 = new TotalRows[1];

            CompanyID[] company;
            List<ACCompany> cmpList;
            AccountingUser aUser = new AccountingUser();

            try
            {

                company = _service.getCompaniesAPI(comp);
                logData = new Dictionary<string, object>();
                logData.Add("company", company);
                WriteToDiagnosticLog($"GetUserCompaniesDetails - result from getCompaniesAPI api", logData);
                cmpList = company.ToGBCompanies();

                if (cmpList == null)
                {
                    WriteToDiagnosticLog($"GetUserCompaniesDetails - returned null data from getCompaniesAPI api - no error ", logData);
                    cmpList = new List<ACCompany>();
                }

               
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"GetUserCompaniesDetails - Error", exception: ex);
                cmpList = new List<ACCompany>();
            }

            return cmpList;
        }


        //
        /// <summary>
        /// Get all the company-properties
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        private List<ACProperty> GetAllCompanyProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {            
            Dictionary<string, object> logData = new Dictionary<string, object>();
                        

            Company[] comp = new Company[1] { new Company() };
            List<NameValuePair> loginInfo = new List<NameValuePair>
            {
                new NameValuePair { Name = "CompanyID", Value = _companyName}, 
                new NameValuePair { Name = "Login", Value = _intactLogin }, 
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };

            if (!String.IsNullOrEmpty(_productUserId))
            {
                loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
            }

            logData = new Dictionary<string, object>();
            logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
            WriteToDiagnosticLog($"GetAllCompanyProperties - _productUserId = {_productUserId}", logData);
            comp[0].NameValuePair = loginInfo.ToArray();


            EntityID[] entitys;
            List<ACProperty> list;

            try
            {
               
                RPObjectCache rpcache = new RPObjectCache();
                //var cacheKey = "GetAllCompanyProperties" + _companyName + _productUserId;
                //entitys = rpcache.GetFromCache<EntityID[]>(cacheKey, 600, () =>
                //{
                //    return _service.getPropertiesAPI(comp);                    
                //});

                entitys = _service.getPropertiesAPI(comp);                    

                logData = new Dictionary<string, object>();
                logData.Add("entity", entitys);

                WriteToDiagnosticLog($"GetAllCompanyProperties - result from getPropertiesAPI api", logData);
                list = entitys.ToGBEnteties();

                if (list == null)
                {
                   list = new List<ACProperty>();
                    WriteToDiagnosticLog($"GetAllCompanyProperties - returned null data from getPropertiesAPI api - no error ", logData);
                }               
               
            }
            catch (Exception ex)
            {
                list = null;
                WriteToErrorLog($"GetAllCompanyProperties - getPropertiesAPI api Error", exception: ex);               
            }

            return list;
        }

        #endregion

        #region Roles

        /// <summary>
        /// Used to get the list of roles for the given user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetUserRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
		{
			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (response.IsError) { return response; }
			FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);

			Role[] role = new Role[1] { new Role() };

			List<NameValuePair> loginInfo = new List<NameValuePair>
				  {
					 new NameValuePair { Name = "CompanyID", Value = _companyName },
					 new NameValuePair { Name = "Login", Value = _intactLogin },
					 new NameValuePair { Name = "Password", Value = _intactPassword }
				 };
			if (!String.IsNullOrEmpty(_productUserId))
			{
				loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
			}
			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"GetUserRoles - _productUserId = {_productUserId}", logData);
			role[0].NameValuePair = loginInfo.ToArray();

			TotalRows[] results2 = new TotalRows[1];
			RoleName[] roleList;
			IList<ProductRole> list;

			try
			{
				roleList = _service.GetAllRoles(role, wsParams, out results2);
				logData = new Dictionary<string, object>();
				logData.Add("roleList", roleList);
				logData.Add("results2", results2);
				WriteToDiagnosticLog($"GetUserRoles - result from api", logData);
				list = roleList.ToGBRoles();

                if (list == null)
                {
                    if (results2.Length > 0)
                    {
                        string message = results2[0].TotalRows1;
                        if (message.ToUpper().Contains("NOT A VALID USERID"))
                        {
                            throw new Exception("Invalid user");
                        }
                    }
                    list = new List<ProductRole>();
                }

                response = new ListResponse()
				{
					Records = list.Cast<object>().ToList(),
					TotalRows = list.Count,
					RowsPerPage = 9999,
					TotalPages = 1,
					ErrorReason = ""
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"GetUserRoles - Error. {ex.Message} ", exception: ex);
				response = new ListResponse();
				response.IsError = true;

				if (ex is BlueBookException)
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

        #endregion

        /// <summary>
        /// Get current companies and assign to user for Allow access to all current and future companies
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="propertiesToAssign"></param>
        /// <param name="isAccountingAdmin"></param>
        /// <returns></returns>
        public string AssignAllCurrentCompaniesToUser(long editorPersonaId, long userPersonaId, List<string> propertiesToAssign, bool isAccountingAdmin, BatchProcessType batchProcessType)
        {
            RequestParameter datafilter = new RequestParameter();
            Dictionary<string, object> logData = new Dictionary<string, object>();
            List<ACCompany> currentCompanyList = GetUserCompaniesDetails(editorPersonaId, userPersonaId, datafilter);
            
            logData = new Dictionary<string, object>();
            logData.Add("currentCompanyList", currentCompanyList);
            WriteToDiagnosticLog($"AssignAllCurrentCompaniesToUser - Current companies to be assigned to user - currentCompanyList", logData);
            propertiesToAssign.Clear();

            foreach (ACCompany company in currentCompanyList)
            {
                propertiesToAssign.Add(company.Id);               
            }

            return UpdatePropertiesToUser(editorPersonaId, userPersonaId, propertiesToAssign, isAccountingAdmin, batchProcessType);            
        }
            

        /// <summary>
        /// Update the properties assigned to the given user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="propertiesToAssign"></param>
        /// <param name="isAccountingAdmin"></param>
        /// <param name="batchProcessType"></param>
        /// <returns></returns>
        public string UpdatePropertiesToUser(long editorPersonaId, long userPersonaId, List<string> propertiesToAssign, bool isAccountingAdmin, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
		{
			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			logData.Add("propertiesToAssign", propertiesToAssign);

			string assignSuccessful = "";
			WriteToDiagnosticLog($"UpdatePropertiesToUser - Begin", logData);
			response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (response.IsError) { return response.ErrorReason; }

			if (String.IsNullOrEmpty(_productUserId))
			{
				WriteToDiagnosticLog($"UpdatePropertiesToUser - Missing product user. _productUserId = empty");
				return "Missing product user";
			}

			RequestParameter datafilter = new RequestParameter();
			logData = new Dictionary<string, object>();
			string propertyIDAddList = "All";
			string propertyIDRemoveList = "";
			List<string> propertiesToRemove = new List<string>();
			bool superUser = IsSuperUser(userPersonaId);
			WriteToDiagnosticLog($"UpdatePropertiesToUser - isSuperUser = {superUser.ToString()}");

            if (batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToExternal || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin ||  batchProcessType == BatchProcessType.UserTypeExternalToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToRegular)
                {
                    WriteToDiagnosticLog($"UpdatePropertiesToUser-BatchProcessType.UserTypeRegularToAdmin - START");
                    propertyIDRemoveList = "";
                    List<ACProperty> currentPropertyList = GetAllCompanyProperties(editorPersonaId, userPersonaId, datafilter);
					IList<ProductPropertyGroup> currentLocationGrpList = GetAllPropertyGroups(editorPersonaId, userPersonaId, datafilter);
					logData = new Dictionary<string, object>();
                    logData.Add("currentPropertyList", currentPropertyList);
                    WriteToDiagnosticLog($"UpdatePropertiesToUser-BatchProcessType.UserTypeRegularToAdmin - currentPropertyList", logData);
                    // Get the current property list what is already assigned and remove them.
                    foreach (ACProperty prop in currentPropertyList)
                    {
                        if (prop.IsAssigned)
                        {
                            if (prop.MConsoleId == string.Empty)
                            {
                                propertiesToRemove.Add(prop.PropertyId);
                            }
                            else
                            {
                                propertiesToRemove.Add(prop.MConsoleId);
                            }
                        }
                    }

					if (currentLocationGrpList != null)
					{
						foreach (ProductPropertyGroup propLG in currentLocationGrpList)
						{
							if ((bool)propLG.IsAssigned)
							{
								propertiesToRemove.Add(propLG.ID);
							}
						}
					}
					
					if (propertiesToRemove.Count > 0)
                    {
                        propertyIDRemoveList = string.Join(",", propertiesToRemove);
                    }

                    WriteToDiagnosticLog($"UpdatePropertiesToUser-BatchProcessType.UserTypeRegularToAdmin - propertiesToRemove = {propertiesToRemove}");
                    propertyIDAddList = "All";
                    WriteToDiagnosticLog($"UpdatePropertiesToUser-BatchProcessType.UserTypeRegularToAdmin - END");
                }

                if (batchProcessType == BatchProcessType.UserTypeAdminToRegular)
                {
                    WriteToDiagnosticLog($"UpdatePropertiesToUser-BatchProcessType.UserTypeAdminToRegular - START");

                    propertyIDAddList = "";
                                                                                
                    if (propertiesToAssign.Count > 0)
                    {
                        propertyIDAddList = string.Join(",", propertiesToAssign);
                    }
                    
                    WriteToDiagnosticLog($"UpdatePropertiesToUser-BatchProcessType.UserTypeAdminToRegular - propertyIDAddList = {propertyIDAddList}");
                    WriteToDiagnosticLog($"UpdatePropertiesToUser-BatchProcessType.UserTypeAdminToRegular - END");
                }
            }
            else
            {
                //if (!superUser && !isAccountingAdmin && propertiesToAssign[0].ToUpper() != "ALL")
                if (!superUser && propertiesToAssign[0].ToUpper() != "ALL")
                {
                    propertyIDAddList = "";
                    List<ACProperty> currentPropertyList =  GetAllCompanyProperties(editorPersonaId, userPersonaId, datafilter);
					IList<ProductPropertyGroup> currentLocationGrpList = GetAllPropertyGroups(editorPersonaId, userPersonaId, datafilter);
					logData = new Dictionary<string, object>();
                    logData.Add("currentPropertyList", currentPropertyList);
                    WriteToDiagnosticLog($"UpdatePropertiesToUser - currentPropertyList", logData);
                    // compare the current property list to what was passed to determine what is new and what was removed.
                    foreach (ACProperty prop in currentPropertyList)
                    {
                        if (prop.PropertyId != string.Empty)
                        {
                            if (!(propertiesToAssign.Contains(prop.PropertyId)))
                            {
                                if (prop.IsAssigned)
                                {
                                    if (prop.MConsoleId == string.Empty)
                                    {
                                        // property doesn't exist, so add it to the list
                                        propertiesToRemove.Add(prop.PropertyId);
                                    }
                                    else
                                    {
                                        propertiesToRemove.Add(prop.MConsoleId);
                                    }
                                }
                            }
                            if (propertiesToAssign.Contains(prop.PropertyId) && prop.IsAssigned)
                            {

                                if (prop.MConsoleId == string.Empty)
                                {
                                    propertiesToAssign.Remove(prop.PropertyId);
                                }
                                else
                                {
                                    propertiesToAssign.Remove(prop.MConsoleId);
                                }
                            }
                        }
                        else
                        {
                            if (!(propertiesToAssign.Contains(prop.CompanyId)))
                            {
                                if (prop.IsAssigned)
                                {
                                    if (prop.MConsoleId == string.Empty)
                                    {
                                        // property doesn't exist, so add it to the list
                                        propertiesToRemove.Add(prop.PropertyId);
                                    }
                                    else
                                    {
                                        propertiesToRemove.Add(prop.MConsoleId);
                                    }
                                }
                            }
                            
                        }
                    }

					if (currentLocationGrpList != null)
					{
						foreach (ProductPropertyGroup propLG in currentLocationGrpList)
						{
							if ((bool)propLG.IsAssigned)
							{
								if (!(propertiesToAssign.Contains(propLG.ID)))
								{
									propertiesToRemove.Add(propLG.ID);
								}
								else
								{
									propertiesToAssign.Remove(propLG.ID);
								}
							}
						}
					}
				
					if (propertiesToAssign.Count > 0)
                    {
                        propertyIDAddList = string.Join(",", propertiesToAssign);
                    }
                    if (propertiesToRemove.Count > 0)
                    {
                        propertyIDRemoveList = string.Join(",", propertiesToRemove);
                    }
                    WriteToDiagnosticLog($"UpdatePropertiesToUser - propertyIDAddList = {propertyIDAddList}");
                    WriteToDiagnosticLog($"UpdatePropertiesToUser - propertyIDRemoveList = {propertyIDRemoveList}");
                }
            }

			NameValuePair[] user = new NameValuePair[4]
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword },
				new NameValuePair { Name = "SystemIdentifier", Value = _productUserId }
			};
			NameValuePair[] newUser = user;
			Array.Resize(ref newUser, newUser.Length + 1);
			newUser[4] = new NameValuePair { Name = "replace", Value = "" };
			user = newUser;
            //if (superUser || isAccountingAdmin)
            if (superUser)
            {
                if ((propertiesToAssign.Count > 0) && (propertiesToAssign[0].ToUpper() != "ALL"))
                {
                    propertyIDAddList = string.Join(",", propertiesToAssign);
                }

                if (batchProcessType != BatchProcessType.UserTypeRegularToAdmin )
                {               
                    // dont need to assign anything because super users get everything automatically in Acounting
                    //propertyIDAddList = "";
                    propertyIDRemoveList = "";
                }
                if (batchProcessType != BatchProcessType.UserTypeExternalToAdmin)
                {                   
                    propertyIDRemoveList = "";
                }
            }

			string result = "";
			try
			{
				if (!string.IsNullOrWhiteSpace(propertyIDRemoveList))
				{
					user[4].Name = "PropertyIdsToRemove";
					user[4].Value = propertyIDRemoveList;
					logData = new Dictionary<string, object>();
					logData.Add("user", RemovePrivateData(user));
					// dont save the password to the log!
					WriteToDiagnosticLog($"UpdatePropertiesToUser - RemovePropertiesFromUser. userPersonaId={userPersonaId}", logData);
                    string json = JsonConvert.SerializeObject(user);
					result = _service.RemovePropertiesFromUser(user);
                    if (result != null && (!result.ToUpper().Contains("PROVIDED USER PROPERTIES REMOVED SUCCESSFULLY") && !result.ToUpper().Contains("PROVIDED USER PROPERTIES DELETED SUCCESSFULLY")))
                    {
						return assignSuccessful += "Failed to remove. " + result;
					}
                    else
                    {
                        assignSuccessful = string.Empty;
                    }
                    WriteToDiagnosticLog($"UpdatePropertiesToUser - RemovePropertiesFromUser. userPersonaId={userPersonaId}. Result={assignSuccessful}");
				}
				if (!string.IsNullOrWhiteSpace(propertyIDAddList))
				{
					user[4].Name = "PropertyIdsToAdd";
					user[4].Value = propertyIDAddList;
					logData = new Dictionary<string, object>();
					logData.Add("user[0]", user[0]);
					logData.Add("user[1]", user[1]);
					logData.Add("user[3]", user[3]);
					logData.Add("user[4]", user[4]);
					//WriteToDiagnosticLog($"UpdatePropertiesToUser - AssignPropertiesToUser. userPersonaId={userPersonaId}", logData);
                    WriteToDiagnosticLog($"UpdatePropertiesToUser  userPersonaId={userPersonaId}- JSON input " + JsonConvert.SerializeObject(logData));
                    result = _service.AssignPropertiesToUser(user);
					if (result != null && !result.ToUpper().Contains("PROVIDED USER PROPERTIES ADDED SUCCESSFULLY"))
					{
						return assignSuccessful += "Failed to assign. " + result;
                    }
                    else
                    {
                        assignSuccessful = string.Empty;
                    }
					WriteToDiagnosticLog($"UpdatePropertiesToUser - AssignPropertiesToUser. userPersonaId={userPersonaId}. Result={assignSuccessful}");
				}
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"UpdatePropertiesToUser - Error", exception: ex);
				return "An error occurred. " + ex.Message;
			}
			WriteToDiagnosticLog($"UpdatePropertiesToUser - Finished");
			return assignSuccessful;
		}

        /// <summary>
        /// Update the roles assigned to the given user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="rolesToAssign"></param>
        /// <param name="isAccountingAdmin"></param>
        /// <param name="batchProcessType"></param>
        /// <returns></returns>
        public string UpdateRolesToUser(long editorPersonaId, long userPersonaId, List<string> rolesToAssign, bool isAccountingAdmin, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
		{
			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();

			string assignSuccessful = "";
			WriteToDiagnosticLog($"UpdateRolesToUser - Begin");
			response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (response.IsError) { return response.ErrorReason; }

			if (String.IsNullOrEmpty(_productUserId))
			{
				WriteToDiagnosticLog($"UpdateRolesToUser - Missing product user. _productUserId = empty");
				return "Missing product user";
			}
			RequestParameter datafilter = new RequestParameter();

			string roleIDAddList = "";
			string roleIDRemoveList = "";
			List<string> rolesToRemove = new List<string>();
			bool superUser = IsSuperUser(userPersonaId);
			WriteToDiagnosticLog($"UpdateRolesToUser - isSuperUser = {superUser.ToString()}");
			ListResponse currentRoleList = GetUserRoles(editorPersonaId, userPersonaId, datafilter);
			logData = new Dictionary<string, object> { { "currentRoleList", currentRoleList } };
			WriteToDiagnosticLog($"UpdateRolesToUser - currentRoleList", logData);

            if (batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToExternal || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                // For RegularToAdmin User REMOVE existing roles and update to ALL
                if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin ||  batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
                {
                    WriteToDiagnosticLog($"UpdateRolesToUser-BatchProcessType.UserTypeRegularToAdmin - START");
                    foreach (ProductRole role in currentRoleList.Records)
                    {                        
                        if (role.IsAssigned)
                        {                                
                            rolesToRemove.Add(role.ID);
                        }                        
                    }

                    if (rolesToRemove.Count > 0)
                    {
                        roleIDRemoveList = string.Join(",", rolesToRemove);
                    }
                    WriteToDiagnosticLog($"UpdateRolesToUser-BatchProcessType.UserTypeRegularToAdmin - roleIDRemoveList = {roleIDRemoveList}");

                    // Add all ADMIN roles 
                    List<ProductRole> currentList = currentRoleList.Records.Cast<ProductRole>().ToList();
                    rolesToAssign = new List<string>();
                    foreach (ProductRole role in currentList)
                    {
                        if (role.Name.ToUpper().Contains("ADMIN") && role.IsAssigned == false)
                        {
                            rolesToAssign.Add(role.ID);
                        }
                    }

                    if (rolesToAssign.Count > 0)
                    {
                        roleIDAddList = string.Join(",", rolesToAssign);
                    }

                    WriteToDiagnosticLog($"UpdateRolesToUser-BatchProcessType.UserTypeRegularToAdmin - roleIDAddList = {roleIDAddList}");

                    WriteToDiagnosticLog($"UpdateRolesToUser-BatchProcessType.UserTypeRegularToAdmin - END");
                }

                if (batchProcessType == BatchProcessType.UserTypeAdminToRegular)
                {
                    WriteToDiagnosticLog($"UpdateRolesToUser-BatchProcessType.UserTypeAdminToRegular - START");
                    // Remove Admin Roles
                    foreach (ProductRole role in currentRoleList.Records)
                    {
                        if (role.Name.ToUpper().Contains("ADMIN") && role.IsAssigned == true)
                        {
                            rolesToRemove.Add(role.ID);
                        }
                    }

                    if (rolesToRemove.Count > 0)
                    {
                        roleIDRemoveList = string.Join(",", rolesToRemove);
                    }

                    WriteToDiagnosticLog($"UpdateRolesToUser-BatchProcessType.UserTypeAdminToRegular - roleIDRemoveList = {roleIDRemoveList}");

                    // Assign the newly passed Roles
                    if (rolesToAssign.Count > 0)
                    {
                        roleIDAddList = string.Join(",", rolesToAssign);
                    }
                    
                    WriteToDiagnosticLog($"UpdateRolesToUser-BatchProcessType.UserTypeAdminToRegular - roleIDAddList = {roleIDAddList}");                    

                    WriteToDiagnosticLog($"UpdateRolesToUser-BatchProcessType.UserTypeAdminToRegular - END");
                }
            }
            else
            {
                //if (!superUser && !isAccountingAdmin)
                if (!superUser)
                {
                    // compare the current role list to what was passed to determine what is new and what was removed.
                    foreach (ProductRole role in currentRoleList.Records)
                    {
                        if (!(rolesToAssign.Contains(role.ID)))
                        {
                            if (role.IsAssigned)
                            {
                                // property doesn't exist, so add it to the list
                                rolesToRemove.Add(role.ID);
                            }
                        }
                        if (rolesToAssign.Contains(role.ID) && role.IsAssigned)
                        {
                            rolesToAssign.Remove(role.ID);
                        }
                    }

                    if (rolesToAssign.Count > 0)
                    {
                        roleIDAddList = string.Join(",", rolesToAssign);
                    }
                    if (rolesToRemove.Count > 0)
                    {
                        roleIDRemoveList = string.Join(",", rolesToRemove);
                    }
                    WriteToDiagnosticLog($"UpdateRolesToUser - roleIDAddList = {roleIDAddList}");
                    WriteToDiagnosticLog($"UpdateRolesToUser - roleIDRemoveList = {roleIDRemoveList}");
                }
                else
                {
                    // get any roles containing the word ADMIN and add them to the administrator
                    List<ProductRole> currentList = currentRoleList.Records.Cast<ProductRole>().ToList();
                    rolesToAssign = new List<string>();
                    foreach (ProductRole role in currentList)
                    {
                        if (role.Name.ToUpper().Contains("ADMIN") && role.IsAssigned == false)
                        {
                            rolesToAssign.Add(role.ID);
                        }
                    }
                    roleIDAddList = string.Join(",", rolesToAssign);
                }
            }

			NameValuePair[] user = new NameValuePair[4]
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword },
				new NameValuePair { Name = "SystemIdentifier", Value = _productUserId }
			};
			NameValuePair[] newUser = user;
			Array.Resize(ref newUser, newUser.Length + 1);
			newUser[4] = new NameValuePair { Name = "replace", Value = "" };
			user = newUser;

			string result = "";
			try
			{
				if (!string.IsNullOrWhiteSpace(roleIDRemoveList))
				{
					user[4].Name = "RoleIdsToRemove";
					user[4].Value = roleIDRemoveList;
					logData = new Dictionary<string, object>();
					logData.Add("user", RemovePrivateData(user));
					// dont save the password to the log!
					WriteToDiagnosticLog($"UpdateRolesToUser - RemoveRolesFromUser. userPersonaId={userPersonaId}", logData);
					result = _service.RemoveRolesFromUser(user);
					WriteToDiagnosticLog($"UpdateRolesToUser - RemoveRolesFromUser. result={result}");
					if (!result.ToUpper().Contains("REMOVED PROVIDED ROLES SUCCESSFULLY")) //PROVIDED USER ROLES REMOVED SUCCESSFULLY
                    {
						return assignSuccessful += "Failed to remove. " + result;
                    }
                    else
                    {
                        assignSuccessful = string.Empty;
                    }
					WriteToDiagnosticLog($"UpdateRolesToUser - RemoveRolesFromUser. userPersonaId={userPersonaId}. Result={assignSuccessful}");
				}
				if (!string.IsNullOrWhiteSpace(roleIDAddList))
				{
					user[4].Name = "RoleIdsToAdd";
					user[4].Value = roleIDAddList;
					logData = new Dictionary<string, object>();
					logData.Add("user[0]", user[0]);
					logData.Add("user[1]", user[1]);
					logData.Add("user[3]", user[3]);
					logData.Add("user[4]", user[4]);
					WriteToDiagnosticLog($"UpdateRolesToUser - AssignRolesToUser. userPersonaId={userPersonaId}", logData);
					result = _service.AssignRolesToUser(user);
					WriteToDiagnosticLog($"UpdateRolesToUser - AssignRolesToUser. result={result}");
					if (!result.ToUpper().Contains("PROVIDED USER ROLES ADDED SUCCESSFULLY"))
					{
						return assignSuccessful += "Failed to assign. " + result;
					}
                    else
                    {
                        assignSuccessful = string.Empty;
                    }
                    WriteToDiagnosticLog($"UpdateRolesToUser - AssignRolesToUser. userPersonaId={userPersonaId}. Result={assignSuccessful}");
				}
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"UpdateRolesToUser - Error", exception: ex);
				return "An error occurred. " + ex.Message;
			}
			WriteToDiagnosticLog($"UpdateRolesToUser - Finished");
			return assignSuccessful;
		}

        /// <summary>
		/// Change user type 
		/// </summary>
        public string ChangeAccountingServiceUserType(long createUserPersonaId, long assignUserPersonaId, List<string> rpList, List<string> PropertyList, List<string> CompanyList, bool isAccountingAdmin, bool isSiteSpendManagementUser, bool isUnRestrictedAccessToProp, BatchProcessType batchProcessType)
        {
            return ManageAccountingUser(createUserPersonaId, assignUserPersonaId, rpList, PropertyList, CompanyList,isAccountingAdmin,isSiteSpendManagementUser,isUnRestrictedAccessToProp, batchProcessType);
        }

        /// <summary>
        /// Updated to create/update a user in Accounting
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="RoleList"></param>
        /// <param name="PropertyList"></param>
        /// <param name="CompanyList"></param>
        /// <param name="isAccountingAdmin"></param>
        /// <param name="isSiteSpendManagementUser"></param>
        /// <param name="isUnRestrictedAccessToProp"></param>
        /// <param name="batchProcessType"></param>
        /// <returns></returns>
        public string ManageAccountingUser(long editorPersonaId, long userPersonaId, List<string> RoleList, List<string> PropertyList, List<string> CompanyList, bool isAccountingAdmin, bool isSiteSpendManagementUser, bool isUnRestrictedAccessToProp,  BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            WriteToDiagnosticLog("Beginning ManageAccountingUser");

            try
            {

                ListResponse listResponse = new ListResponse();
                listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError) { return listResponse.ErrorReason; }

                WriteToDiagnosticLog($"ManageAccountingUser - Accounting Admin = {isAccountingAdmin}, SiteSpendManagementUser/Portal User = {isSiteSpendManagementUser}, Access to Current and Future Properties = {isUnRestrictedAccessToProp} ");

                string accountingLoginName = "";
                //string uniqueIdentifier = "";
                Dictionary<string, object> logData = new Dictionary<string, object>();

                Persona userPersona = _managePersona.GetPersona(userPersonaId);
                Guid realPageId = userPersona.RealPageId;

                IC.Person person = _managePerson.GetPerson(realPageId);

                var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

                bool isSuperUser = IsSuperUser(userPersona.PersonaId);
                WriteToDiagnosticLog($"ManageAccountingUser - isSuperUser = {isSuperUser}");

                // get the email address
                string userEmailAddress = "";
                IList<IC.ElectronicAddress> _addresses = _manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, "");
                if (_addresses.Any(a => a.AddressType?.ToUpper() == "EMAIL" && a.contactMechanismUsageType?.Name.ToUpper() == "PRIMARY"))
                {
                    userEmailAddress = (from a in _addresses where a.AddressType.ToUpper() == "EMAIL" && a.contactMechanismUsageType.Name.ToUpper() == "PRIMARY" select a.AddressString).FirstOrDefault();
                }
                else
                {
                    // this must look like a real email address or Intact will fail to create the user
                    // For user with RegularUser No Email ==> when an email is entered
                    if (_addresses.Any(a => a.AddressType?.ToUpper() == "EMAIL" && a.contactMechanismUsageType?.Name.ToUpper() == "EMAIL"))
                    {
                        userEmailAddress = (from a in _addresses where a.AddressType.ToUpper() == "EMAIL" && a.contactMechanismUsageType.Name.ToUpper() == "EMAIL" select a.AddressString).FirstOrDefault();
                    }
                    else
                    {
                        userEmailAddress = userLogin.LoginName;
                    }
                }
                WriteToDiagnosticLog($"ManageAccountingUser - Before email fix userEmailAddress = {userEmailAddress}");
                // verify email address looks valid, will fail if not
                userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);
                WriteToDiagnosticLog($"ManageAccountingUser - After email fix userEmailAddress = {userEmailAddress}");

                if (string.IsNullOrEmpty(_productUserId))
                {
                    // get a login name that isn't in use for the new user
                    bool foundUserName = false;
                    int incrementor = 0;
					string lastNameNoWhiteSpace = person.LastName.TrimWhiteSpace();
					string newproductUsername = (person.FirstName.TrimWhiteSpace().Substring(0, 1) + lastNameNoWhiteSpace.Substring(0, (lastNameNoWhiteSpace.Length >= 19 ? 19 : lastNameNoWhiteSpace.Length))).ToLower();
                    accountingLoginName = newproductUsername;
                    // give up after 10 tries
                    while (!foundUserName)
                    {
                        if (CheckIfUserLoginIsUsed(_editorPersona.PersonaId, accountingLoginName))
                        {
                            incrementor++;
                            accountingLoginName = newproductUsername + incrementor.ToString();
                        }
                        else
                        {
                            foundUserName = true;
                        }

                        if (incrementor == 10)
                        {
                            // after 10 tries something might be wrong, so bail out.
                            WriteToErrorLog($"ManageAccountingUser - Error checking for username in use {accountingLoginName}");
                            return "An error occurred. Unable to get username.";
                        }
                    }
                    WriteToDiagnosticLog($"ManageAccountingUser - generated accountingLoginName = {accountingLoginName}");
                }
                else
                {
                    WriteToDiagnosticLog($"ManageAccountingUser - used _productUsername = {_productUsername}");
                    accountingLoginName = _productUsername;
                }
                string randomPassword = Guid.NewGuid().ToString().Replace("-", "");

                accountingLoginName = RemoveSpecialCharacter(accountingLoginName);

                List<NameValuePair> parameters = new List<NameValuePair>{
					new NameValuePair { Name = "CompanyID", Value = _companyName },
					new NameValuePair { Name = "Login", Value = _intactLogin },
					new NameValuePair { Name = "Password", Value = _intactPassword },
					new NameValuePair { Name = "LoginId", Value = accountingLoginName },
				};

                string userResultString = "";
                string firstName = person.FirstName.Substring(0, person.FirstName.Length >= 40 ? 40 : person.FirstName.Length);
                string lastName = person.LastName.Substring(0, person.LastName.Length >= 40 ? 40 : person.LastName.Length);


                parameters.Add(new NameValuePair { Name = "ConInfoFirstName", Value = firstName });
                parameters.Add(new NameValuePair { Name = "ConInfoLastName", Value = lastName });
                parameters.Add(new NameValuePair { Name = "ConInfoEmail1", Value = userEmailAddress });
                parameters.Add(new NameValuePair { Name = "ConInfoContactName", Value = "" });
                parameters.Add(new NameValuePair { Name = "Description", Value = firstName + " " + lastName });
                parameters.Add(new NameValuePair { Name = "LoginDisabled", Value = "false" });
                parameters.Add(new NameValuePair { Name = "UnRestricted", Value = isUnRestrictedAccessToProp || isSuperUser ? "true" : "false" });  //Allow access to all current and future properties - Toggle from UI
                parameters.Add(new NameValuePair { Name = "SSOEnabled", Value = "true" });
                parameters.Add(new NameValuePair { Name = "SSOCompanyEnabled", Value = "Enabled" });
                parameters.Add(new NameValuePair { Name = "Visible", Value = "true" });
                parameters.Add(new NameValuePair { Name = "Status", Value = "true" });

                parameters.Add(new NameValuePair { Name = "PortalUser", Value = (isSiteSpendManagementUser == true ? "true" : "false") }); // Site Spend Management User - Portal User - Toggle from UI
                parameters.Add(new NameValuePair { Name = "Admin", Value = (isSuperUser || isAccountingAdmin == true ? "true" : "false") }); // For RealPage Admin || Accounting admin toggle from UI

                if (string.IsNullOrEmpty(_productUserId))
                {
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Running);
                    parameters.Add(new NameValuePair { Name = "UserType", Value = "business user" });
                    parameters.Add(new NameValuePair { Name = "PWDNeverExpires", Value = "true" });
                    parameters.Add(new NameValuePair { Name = "PWDQlyNotEnforced", Value = "true" });

                    NameValuePair[] user = parameters.ToArray();
                    NameValuePair[] userResult;
                    logData = new Dictionary<string, object>();
                    logData.Add("user", RemovePrivateData(user));
                    WriteToDiagnosticLog($"ManageAccountingUser - Creating user. userPersonaId = {userPersonaId}", logData);
                    WriteToDiagnosticLog($"ManageAccountingUser - JSON input - CreateUser " + JsonConvert.SerializeObject(logData));
                    userResult = _service.CreateUser(user);

                    if (userResult[0].Value.ToUpper().Contains("CAN'T CREATE THE USER") || userResult[0].Value.ToUpper().Contains("SECURITY QUESTIONS AND ANSWER COULD NOT BE UPDATED"))
                    {
                        logData = new Dictionary<string, object>();
                        logData.Add("userResult", userResult);
                        WriteToDiagnosticLog($"ManageAccountingUser - Error creating user. userPersonaId = {userPersonaId}", logData);
                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                        return userResult[0].Value;
                    }

                    //_samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, _pmcID);
                    for (int i = 0; i < userResult.Length; i++)
                    {
                        // pull out the needed info
                        string key = userResult[i].Name.ToUpper();
                        switch (key) // SystemIdentifier
                        {
                            case "SYSTEMIDENTIFIER":
                                string pmcuserlogin = userResult[i].Value;
                                _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, pmcuserlogin);
                                _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, pmcuserlogin.Split('|')[1]);
                                WriteToDiagnosticLog($"ManageAccountingUser - Created user. saving product login = {pmcuserlogin}");
                                if (batchProcessType == BatchProcessType.CreateUpdateProductUser)
                                {
                                    WriteCreateUserActivityLog(editorPersonaId, person, userLogin);
                                }
                                var loginInfo = new NameValuePair[4]
                                {
                                new NameValuePair { Name = "CompanyID", Value = _companyName },
                                new NameValuePair { Name = "Login", Value = _intactLogin },
                                new NameValuePair { Name = "Password", Value = _intactPassword },
                                new NameValuePair { Name = "SystemIdentifier", Value = pmcuserlogin }
                                };
                                WriteToDiagnosticLog("ManageAccountingUser.EnableGreenBookUser Begin",
                                    new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo) } }
                                );
                                var message = _service.EnableGreenBookUser(loginInfo);
                                WriteToDiagnosticLog("ManageAccountingUser.EnableGreenBookUser End",
                                    new Dictionary<string, object>() { { "Message", message } }
                                );
                                break;
                        }
                    }
                    // update the users greenbook status

                }
                else
                {
                    if (batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToExternal || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
                    {
                        bool isAdmin = false; // batchProcessType == BatchProcessType.UserTypeRegularToAdmin;
                        if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
                        {
                            isAdmin = true;
                        }
                        //parameters.Add(new NameValuePair { Name = "Admin", Value = (isAdmin == true || isAccountingAdmin == true ? "true" : "false") });

                        WriteToDiagnosticLog($"ManageAccountingUser - BatchProcessType = {batchProcessType.ToString()}");
                        WriteToDiagnosticLog($"ManageAccountingUser - UserType change. isAdmin = {isAdmin}");
                    }

                    parameters.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
                    NameValuePair[] user = parameters.ToArray();
                    logData = new Dictionary<string, object>();
                    logData.Add("user", RemovePrivateData(user));
                    //WriteToDiagnosticLog($"ManageAccountingUser - Updating user. userPersonaId = {userPersonaId}", logData);
                    WriteToDiagnosticLog($"ManageAccountingUser - JSON input - UpdateUser " + JsonConvert.SerializeObject(logData));
                    userResultString = _service.UpdateUser(user);
                    if (batchProcessType == BatchProcessType.CreateUpdateProductUser)
                    {
                        WriteUpdateUserActivityLog(editorPersonaId, person, userLogin);
                    }
                    ChangeStatusAccountingUser(editorPersonaId, userPersonaId, true);
                }

                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
                // check result string results

                if (RoleList == null)
                {
                    RoleList = new List<string>();
                }
                if (PropertyList == null)
                {
                    PropertyList = new List<string>();
                }

                // For SuperUser users -  Accounting sets the Admin related roles - no need to clear prev roles
                if ((!isSuperUser) && RoleList.Count > 0)
                {
                    string updateResultRoles = UpdateRolesToUser(editorPersonaId, userPersonaId, RoleList, isAccountingAdmin, batchProcessType);
                    if (!string.IsNullOrEmpty(updateResultRoles))
                    {
                        return updateResultRoles;
                    }

                }

                // For Accounting Admin users, assign the selected companies. GB-7188
                if(isAccountingAdmin && CompanyList.Count > 0 && PropertyList[0].ToUpper() == "ALL")
                {
                    PropertyList.Clear();
                    PropertyList = CompanyList;                   
                }

                // For SuperUser/IsAccounting Admin users -  Accounting sets ALL properties as unrestricted- no need to clear properties
                if ((!isSuperUser && !isUnRestrictedAccessToProp) && PropertyList.Count > 0)
                {
                    string updateResultProp = UpdatePropertiesToUser(editorPersonaId, userPersonaId, PropertyList, isAccountingAdmin, batchProcessType);                    
                    if (!string.IsNullOrEmpty(updateResultProp))
                    {
                        return updateResultProp;
                    }
                }
                                
                if ((isSuperUser || isUnRestrictedAccessToProp))
                {
                    string updateResultProp = AssignAllCurrentCompaniesToUser(editorPersonaId, userPersonaId, PropertyList, isAccountingAdmin, batchProcessType);
                    if (!string.IsNullOrEmpty(updateResultProp))
                    {
                        return updateResultProp;
                    }
                }

                if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeAdminToExternal || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
                {
                    WriteUpdateUserTypeActivityLog(editorPersonaId, person, userLogin, batchProcessType);
                }

                return "";

            }
            catch (Exception ex)
            {
                WriteToErrorLog($"ManageAccountingUser - Error for user with editorPersona id - {editorPersonaId}", exception: ex);
                return $"Error - {ex.Message}";
            }
        }

        private string RemoveSpecialCharacter(string accountingLoginName)
        {
            switch (accountingLoginName)
            {
                case "portluser":
                case "realpage":
                case "CPAUser":
                case "ExtUser":
                case "SvcUser":
                case "Services":
                case "CNS_":
                    accountingLoginName = $"{accountingLoginName}-1";
                    break;
            }

            var reg = new Regex(@"[^\w\s\-\.]");
            accountingLoginName = reg.Replace(accountingLoginName, string.Empty);

            if(accountingLoginName.Length > 80)
                accountingLoginName = accountingLoginName.Substring(1, 80);

            return accountingLoginName;
        }

        /// <summary>
        /// Update Accounting User Profile
        /// </summary> 
        public string UpdateAccountingUserProfile(long editorPersonaId, long userPersonaId)
        {  

            WriteToDiagnosticLog("Beginning UpdateAccountingUserProfile");
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError) { return listResponse.ErrorReason; }


            string accountingLoginName = "";
            
            Dictionary<string, object> logData = new Dictionary<string, object>();

            Persona userPersona = _managePersona.GetPersona(userPersonaId);
            Guid realPageId = userPersona.RealPageId;

            IC.Person person = _managePerson.GetPerson(realPageId);

            var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

            bool isSuperUser = IsSuperUser(userPersona.PersonaId);
            WriteToDiagnosticLog($"UpdateAccountingUserProfile - isSuperUser = {isSuperUser}");

            // get the email address
            string userEmailAddress = "";
            IList<IC.ElectronicAddress> _addresses = _manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, "");
            if (_addresses.Any(a => a.AddressType?.ToUpper() == "EMAIL" && a.contactMechanismUsageType?.Name.ToUpper() == "PRIMARY"))
            {
                userEmailAddress = (from a in _addresses where a.AddressType.ToUpper() == "EMAIL" && a.contactMechanismUsageType.Name.ToUpper() == "PRIMARY" select a.AddressString).FirstOrDefault();
            }
            else
            {
                // For user with RegularUser No Email ==> when an email is entered
                if (_addresses.Any(a => a.AddressType?.ToUpper() == "EMAIL" && a.contactMechanismUsageType?.Name.ToUpper() == "EMAIL"))
                {
                    userEmailAddress = (from a in _addresses where a.AddressType.ToUpper() == "EMAIL" && a.contactMechanismUsageType.Name.ToUpper() == "EMAIL" select a.AddressString).FirstOrDefault();
                }
                else
                {
                    // this must look like a real email address or Intact will fail to create the user
                    userEmailAddress = userLogin.LoginName;
                }
            }
            WriteToDiagnosticLog($"UpdateAccountingUserProfile - Before email fix userEmailAddress = {userEmailAddress}");
            // verify email address looks valid, will fail if not
            userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);
            WriteToDiagnosticLog($"UpdateAccountingUserProfile - After email fix userEmailAddress = {userEmailAddress}");
            if (!string.IsNullOrEmpty(_productUserId))            
            {
                WriteToDiagnosticLog($"UpdateAccountingUserProfile - used _productUsername = {_productUsername}");
                accountingLoginName = _productUsername;
            }
            
            List<NameValuePair> parameters = new List<NameValuePair>{
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword },
                new NameValuePair { Name = "LoginId", Value = accountingLoginName },
            };
            string result = "";
            string firstName = person.FirstName.Substring(0, person.FirstName.Length >= 40 ? 40 : person.FirstName.Length);
            string lastName = person.LastName.Substring(0, person.LastName.Length >= 40 ? 40 : person.LastName.Length);

            parameters.Add(new NameValuePair { Name = "FirstName", Value = firstName });
            parameters.Add(new NameValuePair { Name = "LastName", Value = lastName });
            parameters.Add(new NameValuePair { Name = "Email", Value = userEmailAddress });
            parameters.Add(new NameValuePair { Name = "Description", Value = firstName + " " + lastName });

            if (!string.IsNullOrEmpty(_productUserId))
            {                           
                parameters.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
                NameValuePair[] user = parameters.ToArray();
                logData = new Dictionary<string, object>();
                logData.Add("user", RemovePrivateData(user));
                //WriteToDiagnosticLog($"UpdateAccountingUserProfile - Updating user. userPersonaId = {userPersonaId}", logData);
                WriteToDiagnosticLog($"UpdateAccountingUserProfile - Updating user. userPersonaId = {userPersonaId} JSON input " + JsonConvert.SerializeObject(logData));
                result = _service.UpdateUserDetails(user);

                if (result.Trim().ToUpper().Contains("SUCCESSFULLY"))
                {                   
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
                    WriteToDiagnosticLog($"Updated profile successfully userPersonaId:{userPersonaId}");

                    // Activity Logging
                    WriteActivityLogWithMessage(editorPersonaId, userPersonaId, "Updated User profile in Financial Suite.");
                }
                else
                {
                    WriteToDiagnosticLog($"Updated User profile in Financial Suite failed userPersonaId:{userPersonaId}");
                    return "Update Profile failed. " + result;
                }                
            }
            
            // check result string results                      

            return "";
        }

        private object RemovePrivateData(NameValuePair[] user)
		{
			user = user.Where(x => x.Name.ToUpper() != "PASSWORD").ToArray();
			return user;
		}

		/// <summary>
		/// Used to enable/disable an Accounting user
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="isActive"></param>
		/// <returns></returns>
		public string ChangeStatusAccountingUser(long editorPersonaId, long userPersonaId, bool isActive)
		{
			ListResponse listResponse = new ListResponse();
			listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (listResponse.IsError || _productUsername == null) { return listResponse.ErrorReason; }
			Dictionary<string, object> logData = new Dictionary<string, object>();

			try
			{
				List<NameValuePair> parameters = new List<NameValuePair>{
					new NameValuePair { Name = "CompanyID", Value = _companyName },
					new NameValuePair { Name = "Login", Value = _intactLogin },
					new NameValuePair { Name = "Password", Value = _intactPassword },
					new NameValuePair { Name = "SystemIdentifier", Value = _productUserId }
				};
				logData = new Dictionary<string, object>();
				logData.Add("parameters", RemovePrivateData(parameters.ToArray()));
				WriteToDiagnosticLog($"ChangeStatusAccountingUser - Updating user status. userPersonaId = {userPersonaId}, isActive = {isActive.ToString()}", logData);
				string result = "";
				if (isActive)
				{
					result = _service.EnableUser(parameters.ToArray());
					UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
				}
				else
				{
					result = _service.DisableUser(parameters.ToArray());
					UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Inactive);
				}
				return result;
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"ChangeStatusAccountingUser - Updating user status. userPersonaId = {userPersonaId}, isActive = {isActive.ToString()}", exception: ex);
				return "Updated failed";
			}
		}

		/// <summary>
		/// Used to enable/disable an Accounting user
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="isLinked"></param>
		/// <returns></returns>
		public bool ChangeAccountingUserClaimStatus(long editorPersonaId, long userPersonaId, bool isLinked)
		{
			ListResponse listResponse = new ListResponse();
			listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (listResponse.IsError || _productUsername == null) { return false; }
			Dictionary<string, object> logData = new Dictionary<string, object>();

			bool result = false;

			try
			{
				logData = new Dictionary<string, object>();
				logData.Add("ProductUserId", _productUserId);
				WriteToDiagnosticLog($"ChangeAccountingUserClaimStatus - ChangeClaimStatus. userPersonaId = {userPersonaId}, isLinked = {isLinked.ToString()}", logData);
				_service.ChangeClaimStatus(_productUserId, isLinked, _intactLogin, _intactPassword, _productUsername);
				result = true;
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"ChangeStatusAccountingUser - Updating user status. userPersonaId = {userPersonaId}, isActive = {isLinked.ToString()}", exception: ex);
			}
			return result;
		}

		/// <summary>
		/// Used to delete an Accounting user
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <returns></returns>
		public string DeleteAccountingUser(long editorPersonaId, long userPersonaId)
		{
			ListResponse listResponse = new ListResponse();
			listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (listResponse.IsError) { return listResponse.ErrorReason; }

			// the Accounting user deleting the user
			try
			{
				List<NameValuePair> parameters = new List<NameValuePair>{
					new NameValuePair { Name = "CompanyID", Value = _companyName },
					new NameValuePair { Name = "Login", Value = _intactLogin },
					new NameValuePair { Name = "Password", Value = _intactPassword },
					new NameValuePair { Name = "SystemIdentifier", Value = _productUserId }
				};
				_service.DeleteUser(parameters.ToArray());
				// now remove the attributes from this persona so a new user can be created later
				UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"DeleteAccountingUser - Delete user. userPersonaId = {userPersonaId}", exception: ex);
				return "There was a problem deleting the user";
			}
			return "";
		}



		#region Roles & Rights

		/// <summary>
		/// Used to get the list of roles and the count of rights associated to that role 
		/// </summary>
		/// <param name="editorPersonaId"></param>        
		/// <param name="datafilter"></param>
		/// <returns></returns>
		public ListResponse GetRolesCount(long editorPersonaId, RequestParameter datafilter)
		{

			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
			if (response.IsError) { return response; }

			Permissions[] permissions = new Permissions[1] { new Permissions() };

			List<NameValuePair> loginInfo = new List<NameValuePair>
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword }
			};

			if (!String.IsNullOrEmpty(_productUserId))
			{
				loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
			}

			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"GetRolesCount - _productUserId = {_productUserId}", logData);
			permissions[0].NameValuePair = loginInfo.ToArray();

			PermissionID[] permissionList;
			IList<ProductRole> list;

			try
			{
                
                permissionList = _service.GetApplicationPermissions(permissions);
				logData = new Dictionary<string, object>();
				logData.Add("roleList", permissionList);

				WriteToDiagnosticLog($"GetRolesCount - result from api", logData);
				list = permissionList.ToRoles();

				response = new ListResponse()
				{
					Records = list.Cast<object>().ToList(),
					TotalRows = list.Count,
					RowsPerPage = 9999,
					TotalPages = 1,
					ErrorReason = ""
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"GetRolesCount - Error", exception: ex);
				response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}

			return response;
		}

		/// <summary>
		/// Used to get the list of ALL roles 
		/// </summary>
		/// <param name="editorPersonaId"></param>        
		/// <param name="datafilter"></param>
		/// <returns></returns>
		public ListResponse GetAllRoles(long editorPersonaId, RequestParameter datafilter)
		{
			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
			if (response.IsError) { return response; }
			FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);

			Role[] role = new Role[1] { new Role() };

			List<NameValuePair> loginInfo = new List<NameValuePair>
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword }
			};

			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"GetALLRoles - _productUserId = {_productUserId}", logData);
			role[0].NameValuePair = loginInfo.ToArray();

			TotalRows[] results2 = new TotalRows[1];
			RoleName[] roleList;
			IList<ProductRole> list;

			try
			{
                WriteToDiagnosticLog($"GetALLRoles - JSON input " + JsonConvert.SerializeObject(wsParams));
                roleList = _service.GetAllRoles(role, wsParams, out results2);
				logData = new Dictionary<string, object>();
				logData.Add("roleList", roleList);
				logData.Add("results2", results2);
				WriteToDiagnosticLog($"GetALLRoles - result from api", logData);
				//list = roleList.ToRoles();

				response = new ListResponse()
				{
					Records = roleList.Cast<object>().ToList(),
					TotalRows = roleList.Length,
					RowsPerPage = 9999,
					TotalPages = 1,
					ErrorReason = ""
				};
			}
			catch (Exception ex)
			{				
				WriteToErrorLog($"GetALLRoles - Error", exception: ex);
				response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}
			return response;
		}


		/// <summary>
		/// Used to get the list of rights 
		/// </summary>
		/// <param name="editorPersonaId"></param>        
		/// <returns></returns>        
		public ListResponse GetRights(long editorPersonaId)
		{
			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
			if (response.IsError) { return response; }

			Permissions[] permissions = new Permissions[1] { new Permissions() };

			List<NameValuePair> loginInfo = new List<NameValuePair>
			{
					 new NameValuePair { Name = "CompanyID", Value = _companyName },
					 new NameValuePair { Name = "Login", Value = _intactLogin },
					 new NameValuePair { Name = "Password", Value = _intactPassword }                     
            };

			if (!String.IsNullOrEmpty(_productUserId))
			{
				loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
			}

			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"GetRights - _productUserId = {_productUserId}", logData);
			permissions[0].NameValuePair = loginInfo.ToArray();

			PermissionID[] permissionList;
			IList<ProductRightAcct> list;

			try
			{

				permissionList = _service.GetApplicationPermissions(permissions);
                
				logData = new Dictionary<string, object>();
				logData.Add("roleList", permissionList);

				WriteToDiagnosticLog($"GetRights - result from api", logData);
				list = permissionList.ToRights();

				response = new ListResponse()
				{
					Records = list.Cast<object>().ToList(),
					TotalRows = list.Count,
					RowsPerPage = 9999,
					TotalPages = 1,
					ErrorReason = ""
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"GetRights - Error", exception: ex);
				response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}
			return response;
		}


		/// <summary>
		/// Used to get the list of applications / modules
		/// </summary>
		/// <param name="editorPersonaId"></param>       
		/// <returns></returns>
		public ListResponse GetApplications(long editorPersonaId)
		{
			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
			if (response.IsError) { return response; }


			Applications[] applications = new Applications[1] { new Applications() };

			List<NameValuePair> loginInfo = new List<NameValuePair>
				  {
                new NameValuePair { Name = "CompanyID", Value = _companyName },
                new NameValuePair { Name = "Login", Value = _intactLogin },
                new NameValuePair { Name = "Password", Value = _intactPassword }
            };
			if (!String.IsNullOrEmpty(_productUserId))
			{
				//loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
			}
			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"GetApplications - _productUserId = {_productUserId}", logData);
			applications[0].NameValuePair = loginInfo.ToArray();

			TotalRows[] results2 = new TotalRows[1];
			ApplicationID[] appList;
			string[] list;

			try
			{
                RPObjectCache rpcache = new RPObjectCache();
                var cacheKey = "AccountingApplications_" + _companyName;
                appList = rpcache.GetFromCache<ApplicationID[]>(cacheKey, 600, () =>
                {
                    return _service.GetApplications(applications);
                });


                //appList = _service.GetApplications(applications);
				logData = new Dictionary<string, object>();
				logData.Add("appList", appList);

				WriteToDiagnosticLog($"GetApplications - result from api", logData);

				list = appList.ToCenters();

				response = new ListResponse()
				{
					Records = list.Cast<object>().ToList(),
					TotalRows = list.Length,
					RowsPerPage = 9999,
					TotalPages = 1,
					ErrorReason = ""
				};
			}
			catch (Exception ex)
			{				
				WriteToErrorLog($"GetApplications - Error", exception: ex);
				response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}
			return response;
		}


		/// <summary>
		/// Used to get a list of roles associated to the given right 
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="rightId"></param>
		/// <param name="assignedOnly"></param>
		/// <param name="datafilter"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public ListResponse GetRolesForRight(long editorPersonaId, RequestParameter datafilter, int rightId, bool assignedOnly, ProductRightAcct right)
		{
			//RoleList roleListResult = new RoleList();
			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
			if (response.IsError) { return response; }


			Permissions[] permissions = new Permissions[1] { new Permissions() };
			FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);
			List<NameValuePair> loginInfo = new List<NameValuePair>
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword }
			};

			if (!String.IsNullOrEmpty(right.ModuleID))
			{
				loginInfo.Add(new NameValuePair { Name = "ModuleID", Value = right.ModuleID });
			}
			if (!String.IsNullOrEmpty(right.ID.ToString()))
			{
				loginInfo.Add(new NameValuePair { Name = "rightID", Value = right.RightID.ToString() });
			}
			if (!String.IsNullOrEmpty(right.Right))
			{
				loginInfo.Add(new NameValuePair { Name = "right", Value = right.Right });
			}
			if (!String.IsNullOrEmpty(right.Action))
			{
				loginInfo.Add(new NameValuePair { Name = "action", Value = right.Action });
			}
			if (!String.IsNullOrEmpty(right.Alias))
			{
				loginInfo.Add(new NameValuePair { Name = "actionLabel", Value = right.ActionLabel });
			}

			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"GetRolesForRight - _productUserId = {_productUserId}", logData);
			permissions[0].NameValuePair = loginInfo.ToArray();

			TotalRows[] results2 = new TotalRows[1];
			PermissionuID[] permissionList;
			IList<ProductRole> list;

			try
			{

				permissionList = _service.GetPermissionRoles(permissions);
				logData = new Dictionary<string, object>();
				logData.Add("roleList", permissionList);
				logData.Add("results2", results2);
				WriteToDiagnosticLog($"GetRolesForRight - result from api", logData);
				list = permissionList.ToRolesList();

				response = new ListResponse()
				{
					Records = list.Cast<object>().ToList(),
					TotalRows = list.Count,
					RowsPerPage = 9999,
					TotalPages = 1,
					ErrorReason = ""
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"GetRolesForRight - Error", exception: ex);
				response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}

			return response;
		}

		/// <summary>
		/// Used to assign or unassign a right to a list of roles
		/// </summary>
		/// <param name="editorPersonaId">The persona of the user making the change. Used to log the OneSite user making the change.</param>
		/// <param name="rightId">The right being assigned</param>
		/// <param name="rolesToAdd">A list of role ids to add to the role</param>
		/// <param name="rolesToRemove">A list of role ids to remove from the role</param>
		/// <param name="right"></param>
		public ListResponse UpdateRolesForRight(long editorPersonaId, int rightId, List<ProductRoleAcct> rolesToAdd, List<ProductRoleAcct> rolesToRemove, ProductRightAcct right)
		{

			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
			if (response.IsError) { return response; }

			int arrLength = rolesToAdd.Count + rolesToRemove.Count;
			RolePermission[] rolePermissions = new RolePermission[arrLength];
			RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSiteAccounting.User[] user = new RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSiteAccounting.User[1] { new RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSiteAccounting.User() };

			List<NameValuePair> loginInfo = new List<NameValuePair>
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword }
			};

			if (!String.IsNullOrEmpty(_productUserId))
			{
				loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
			}

			int i = 0;
			foreach (var item in rolesToAdd)
			{
				RolePermission rp = new RolePermission();

				rp.moduleid = right.ModuleID;
				rp.right = right.Right;
				rp.action = right.Action;
				//rp.roleid = item.ID.ToString();
				rp.roleName = item.Name;
				rp.value = "true";
				rolePermissions[i] = rp;
				i++;
			}

			foreach (var item in rolesToRemove)
			{
				RolePermission rp = new RolePermission();

				rp.moduleid = right.ModuleID;
				rp.right = right.Right;
				rp.action = right.Action;
				//rp.roleid = item.ID.ToString();
				rp.roleName = item.Name;
				rp.value = "false";
				rolePermissions[i] = rp;
				i++;
			}



			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"UpdateRolesForRight - _productUserId = {_productUserId}", logData);
			user[0].NameValuePair = loginInfo.ToArray();

			NameValuePair[] output;

			try
			{
                WriteToDiagnosticLog($"UpdateRolesForRight - JSON input " + JsonConvert.SerializeObject(rolePermissions) );
                output = _service.AssignRolePermissions(user, rolePermissions);
                WriteToDiagnosticLog($"UpdateRolesForRight - result from api", logData);
				logData = new Dictionary<string, object>();
				logData.Add("output", output);

				WriteToDiagnosticLog($"UpdateRolesForRight - result from api", logData);

				string error = string.Empty;
				bool isError = false;

				if (output[0].Value.IndexOf("fail") != -1)
				{
					error = output[1].Value;
					isError = true;
				}

				response = new ListResponse()
				{
					Records = output.Cast<object>().ToList(),
					TotalRows = output.Length,
					RowsPerPage = 9999,
					TotalPages = 1,
					ErrorReason = ""
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"UpdateRolesForRight - Error", exception: ex);
				response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}
			return response;
		}

		/// <summary>
		/// Used to get a list of rights associated to the given role id
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="roleId"></param>        
		/// <param name="datafilter"></param>
		/// <returns></returns>
		public ListResponse GetRightsForRole(long editorPersonaId, RequestParameter datafilter, string roleName, int roleId = 0)
		{
			//RoleList roleListResult = new RoleList();
			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
			if (response.IsError) { return response; }


			Permissions[] permissions = new Permissions[1] { new Permissions() };
			FilterSortParameters wsParams = ManageProductOneSiteAccountingHelpers.GenerateSearchAndPaging(datafilter, "Name", 0, 9999);
			List<NameValuePair> loginInfo = new List<NameValuePair>
		    {
			    new NameValuePair { Name = "CompanyID", Value = _companyName },
			    new NameValuePair { Name = "Login", Value = _intactLogin },
			    new NameValuePair { Name = "Password", Value = _intactPassword }
		    };


			if (!String.IsNullOrEmpty(_productUserId))
			{
				loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
			}
			if (!String.IsNullOrEmpty(roleId.ToString()))
			{
				loginInfo.Add(new NameValuePair { Name = "RoleName", Value = roleName });
			}

			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"GetRightsForRole - _productUserId = {_productUserId}", logData);
			permissions[0].NameValuePair = loginInfo.ToArray();

			TotalRows[] results2 = new TotalRows[1];
			PermissionID[] roleList;
			IList<ProductRightAcct> list;

			try
			{

				roleList = _service.GetRolePermissions(permissions);
				logData = new Dictionary<string, object>();
				logData.Add("roleList", roleList);

				WriteToDiagnosticLog($"GetRightsForRole - result from api", logData);
				list = roleList.ToRights();

				response = new ListResponse()
				{
					Records = list.Cast<object>().ToList(),
					TotalRows = list.Count,
					RowsPerPage = 9999,
					TotalPages = 1,
					ErrorReason = ""
				};
			}
			catch (Exception ex)
			{				
				WriteToErrorLog($"GetRightsForRole - Error", exception: ex);
				response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}

			return response;
		}


		/// <summary>
		/// Used to assign or unassign a right to a list of roles
		/// </summary>
		/// <param name="editorPersonaId">The persona of the user making the change. Used to log the OneSite user making the change.</param>
		/// <param name="roleId">The role being assigned</param>
		/// <param name="rightsToAdd">A list of right ids to add to the role</param>
		/// <param name="rightsToRemove">A list of right ids to remove from the role</param>
		public ListResponse UpdateRightsForRole(long editorPersonaId, int roleId, string roleName, List<ProductRightAcct> rightsToAdd, List<ProductRightAcct> rightsToRemove)
		{


			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
			if (response.IsError) { return response; }

			int arrLength = rightsToAdd.Count + rightsToRemove.Count;
			RolePermission[] rolePermissions = new RolePermission[arrLength];
			RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSiteAccounting.User[] user = new RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSiteAccounting.User[1] { new RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSiteAccounting.User() };

			List<NameValuePair> loginInfo = new List<NameValuePair>
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword }
			};

			if (!String.IsNullOrEmpty(_productUserId))
			{
				loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
			}

			int i = 0;
			foreach (var item in rightsToAdd)
			{
				RolePermission rp = new RolePermission();

				rp.moduleid = item.ModuleID;
				rp.right = item.Right;
				rp.action = item.Action;
				//rp.roleid = roleId.ToString();
				rp.roleName = roleName;
				rp.value = "true";
				rolePermissions[i] = rp;
				i++;
			}

			foreach (var item in rightsToRemove)
			{
				RolePermission rp = new RolePermission();

				rp.moduleid = item.ModuleID;
				rp.right = item.Right;
				rp.action = item.Action;
				//rp.roleid = roleId.ToString();
				rp.roleName = roleName;
				rp.value = "false";
				rolePermissions[i] = rp;
				i++;
			}



			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"UpdateRightsForRole - _productUserId = {_productUserId}", logData);
			user[0].NameValuePair = loginInfo.ToArray();

			NameValuePair[] output;

			try
			{
                WriteToDiagnosticLog($"UpdateRightsForRole - JSON input " + JsonConvert.SerializeObject(rolePermissions));
                output = _service.AssignRolePermissions(user, rolePermissions);
				logData = new Dictionary<string, object>();
				logData.Add("output", output);

				WriteToDiagnosticLog($"UpdateRightsForRole - result from api", logData);

				string error = string.Empty;
				bool isError = false;

				if (output[0].Value.IndexOf("fail") != -1)
				{
					error = "Error - Unable to assign rights"; //output[1].Value;
					isError = true;
				}

				response = new ListResponse()
				{
					Records = output.Cast<object>().ToList(),
					TotalRows = output.Length,
					RowsPerPage = 9999,
					TotalPages = 1,
					ErrorReason = error,
					IsError = isError
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"UpdateRightsForRole - Error", exception: ex);
				response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}
			return response;
		}

		/// <summary>
		/// Used to add/update a role in OneSite Accounting
		/// </summary>
		/// <param name="editorPersonaId">The persona of the user making the change. Used to log the OneSite Accounting user making the change.</param>       
		/// <param name="roleName"></param>        
		/// <returns></returns>
		public ListResponse CreateRole(long editorPersonaId, string roleName)
		{

			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
			if (response.IsError) { return response; }

			NameValuePair[] input = new NameValuePair[1] { new NameValuePair() };

			List<NameValuePair> loginInfo = new List<NameValuePair>
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword }
			};

			if (!String.IsNullOrEmpty(_productUserId))
			{
				loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
			}

			if (!String.IsNullOrEmpty(roleName))
			{
				loginInfo.Add(new NameValuePair { Name = "Name", Value = roleName });
				loginInfo.Add(new NameValuePair { Name = "Description", Value = "" });
			}

			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"CreateRole - _productUserId = {_productUserId}", logData);
			input = loginInfo.ToArray();

			NameValuePair[] output;
			IList<ProductRightAcct> list;

			try
			{

				output = _service.CreateRole(input);
				logData = new Dictionary<string, object>();
				logData.Add("output", output);

				WriteToDiagnosticLog($"CreateRole - result from api", logData);

				string error = string.Empty;
				bool isError = false;


				if (output[0].Name.IndexOf("Error") != -1)
				{
					error = output[0].Value;
					isError = true;
				}

				response = new ListResponse()
				{
					Records = output.Cast<object>().ToList(),
					TotalRows = output.Length,
					RowsPerPage = 9999,
					TotalPages = 1,
					ErrorReason = error,
					IsError = isError
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"CreateRole - Error", exception: ex);
				response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}
			return response;
		}

		/// <summary>
		/// Used to Delete a role in Onesite accounting
		/// </summary>
		/// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>       
		/// <param name="roleId"></param>
		/// <returns></returns>
		public ListResponse DeleteRole(long editorPersonaId, long roleId, string roleName)
		{
			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
			if (response.IsError) { return response; }

			NameValuePair[] input = new NameValuePair[1] { new NameValuePair() };

			List<NameValuePair> loginInfo = new List<NameValuePair>
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword }
			};

			if (!String.IsNullOrEmpty(_productUserId))
			{
				loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
			}

			if (!String.IsNullOrEmpty(roleId.ToString()))
			{
				//loginInfo.Add(new NameValuePair { Name = "RoleID", Value = roleId.ToString() });
				loginInfo.Add(new NameValuePair { Name = "Name", Value = roleName });
			}

			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"DeleteRole - _productUserId = {_productUserId}", logData);
			input = loginInfo.ToArray();

			NameValuePair[] output;
			IList<ProductRightAcct> list;

			try
			{

				output = _service.DeleteRole(input);
				logData = new Dictionary<string, object>();
				logData.Add("output", output);

				WriteToDiagnosticLog($"DeleteRole - result from api", logData);

				string error = string.Empty;
				bool isError = false;


				if (output[0].Name.IndexOf("Error") != -1)
				{
					//error = output[0].Value;
					error = "Role cannot be deleted because it is currently in use";
					isError = true;
				}

				response = new ListResponse()
				{
					Records = output.Cast<object>().ToList(),
					TotalRows = output.Length,
					RowsPerPage = 9999,
					TotalPages = 1,
					ErrorReason = error,
					IsError = isError
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"DeleteRole - Error", exception: ex);
				response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}
			return response;
		}


		/// <summary>
		/// Used to Clone a role in Onesite accounting
		/// </summary>
		/// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>       
		/// <param name="inheritedRoleName"></param>
		/// <param name="roleName"></param>        
		/// <returns></returns>
		public ListResponse CloneRole(long editorPersonaId, string roleName, string inheritedRoleName)
		{
			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
			if (response.IsError) { return response; }

			NameValuePair[] input = new NameValuePair[1] { new NameValuePair() };

			List<NameValuePair> loginInfo = new List<NameValuePair>
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword }
			};

			if (!String.IsNullOrEmpty(_productUserId))
			{
				loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
			}


			loginInfo.Add(new NameValuePair { Name = "NewName", Value = roleName });
            loginInfo.Add(new NameValuePair { Name = "Description", Value = "" });
			loginInfo.Add(new NameValuePair { Name = "Name", Value = inheritedRoleName });


			logData = new Dictionary<string, object>();
			logData.Add("user", RemovePrivateData(loginInfo.ToArray()));
			WriteToDiagnosticLog($"CloneRole - _productUserId = {_productUserId}", logData);
			input = loginInfo.ToArray();

			NameValuePair[] output;
			IList<ProductRightAcct> list;

			try
			{

				output = _service.CreateRole(input);
				logData = new Dictionary<string, object>();
				logData.Add("output", output);

				WriteToDiagnosticLog($"CloneRole - result from api", logData);

				string error = string.Empty;
				bool isError = false;


				if (output[0].Name.IndexOf("Error") != -1)
				{
					error = output[0].Value;
					isError = true;
				}

				response = new ListResponse()
				{
					Records = output.Cast<object>().ToList(),
					TotalRows = output.Length,
					RowsPerPage = 9999,
					TotalPages = 1,
					ErrorReason = error,
					IsError = isError
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"CloneRole - Error", exception: ex);
				response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}
			return response;
		}


        #endregion


        #region Private

        private List<int> GetProductIdsByOrg()
        {
            ProductRepository pr = new ProductRepository();
            return  (List<int>)pr.GetProductIdsByCompany(_userClaims.OrganizationRealPageGuid);
        }

        /// <summary>
        /// Used to see if a new user login being added already exists or not
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        private bool CheckIfUserLoginIsUsed(long editorPersonaId, string userLogin)
		{
			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();

			response = GetCompanyEditorAndUserDetails(editorPersonaId, 0);

			bool userExists = false;

			List<NameValuePair> loginInfo = new List<NameValuePair>
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword },
				new NameValuePair { Name = "UserID", Value = userLogin },
			};

			NameValuePair[] user = loginInfo.ToArray();
			logData.Add("user", RemovePrivateData(user));
			WriteToDiagnosticLog("CheckIfUserLoginIsUsed", logData);
			string result = "";
			try
			{
				result = _service.CheckIfUserIDIsUsed(user);
				WriteToDiagnosticLog($"CheckIfUserLoginIsUsed result={result}");
				if (result.ToUpper() == "YES")
				{
					userExists = true;
				}
			}
			catch (Exception ex)
			{
				WriteToDiagnosticLog($"CheckIfUserLoginIsUsed exception. ex={ex.Message}");
				// return the user exists
				return true;
			}
			return userExists;
		}

		/// <summary>
		/// Used to get information about the calling user and user being modified
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <returns></returns>
		private new ListResponse GetCompanyEditorAndUserDetails(long editorPersonaId, long userPersonaId)
		{
			ListResponse response = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			WriteToDiagnosticLog($"GetCompanyEditorAndUserDetails - Begin");
			response = verifyPersona(editorPersonaId);
			if (response.IsError)
			{
				return response;
			}
			else
			{
				// get the editors persona from the result
				_editorPersona = response.Records[0] as Persona;
			}

			_companyName = GetAccountingCompanyFromPersona(_editorPersona);
			if (string.IsNullOrEmpty(_companyName))
			{
				response.IsError = true;
				response.ErrorReason = "Missing company name";
				WriteToDiagnosticLog($"GetCompanyEditorAndUserDetails - Missing company name. _editorPersona={_editorPersona}");
				return response;
			}
			WriteToDiagnosticLog($"GetCompanyEditorAndUserDetails - _companyName={_companyName}");
			if (userPersonaId != 0)
			{
				// verify the persona being changed belongs to the same company as the user making the changes
				Persona user = _managePersona.GetPersona(userPersonaId);
				if (user == null || user.Organization.PartyId != _editorPersona.Organization.PartyId)
				{
					response.IsError = true;
					response.ErrorReason = "Invalid user persona";
					WriteToDiagnosticLog($"GetCompanyEditorAndUserDetails - Error invalid user persona. userPersonaId={userPersonaId}");
					return response;
				}
				IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(userPersonaId, _productId);
				logData = new Dictionary<string, object>();
				logData.Add("productAttributes", productAttributes);
				WriteToDiagnosticLog($"GetCompanyEditorAndUserDetails - userPersonaId={userPersonaId}", logData);
				// the Accounting user making the change to the role, get the Company from the user
				if (productAttributes.Any(a => a.Name.ToUpper() == "PRODUCTUSERNAME"))
				{
					_productUsername = (from a in productAttributes where a.Name.ToUpper() == "PRODUCTUSERNAME" select a.Value).FirstOrDefault().Replace(":", "|");
					WriteToDiagnosticLog($"GetCompanyEditorAndUserDetails - _productUsername={_productUsername}");
				}
				if (productAttributes.Any(a => a.Name.ToUpper() == "USERID"))
				{
					_productUserId = (from a in productAttributes where a.Name.ToUpper() == "USERID" select a.Value).FirstOrDefault().Replace(":", "|");
					WriteToDiagnosticLog($"GetCompanyEditorAndUserDetails - _productUserId={_productUserId}");
				}
			}
			WriteToDiagnosticLog($"GetCompanyEditorAndUserDetails - Finished");
			return response;
		}

		/// <summary>
		/// Get the Accounting CompanyName for the admin user
		/// </summary>
		/// <param name="persona"></param>
		/// <returns></returns>
		private string GetAccountingCompanyFromPersona(Persona persona)
		{
			string companyName = "";
			IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(persona.PersonaId, _productId);
			// the Accounting user making the change to the role, get the Company from the user
			string uniqueIdentifier = (from a in productAttributes where a.Name.ToUpper() == "USERID" select a.Value).FirstOrDefault();
			if (uniqueIdentifier == null)
			{
                // get the CompanyName from BlueBook because the user doesn't have the Company for Accounting yet
                //IList<CompanyMap> companyMap = _blueBook.GetCompanyMap(persona.Organization.BooksMasterId, BlueBookProductConstants.Accounting);
                IList<blueBook.CustomerCompanyMap> companyMap = _blueBook.GetCompanyMap(persona.Organization.RealPageId, persona.Organization.BooksCustomerMasterId, source: BlueBookProductConstants.FinancialSuite, domain: persona.Organization.OrganizationDomain.Name);

                if (companyMap != null && companyMap.Count > 0 && companyMap.First(a => a.Source.ToUpper() == BlueBookProductConstants.FinancialSuite) != null)
				{
					companyName = companyMap.First(a => a.Source.ToUpper() == BlueBookProductConstants.FinancialSuite).CompanyInstanceSourceId;
				}
			}
			else
			{
				companyName = uniqueIdentifier.Split('|')[0];
			}
			return companyName;
		}



		#endregion

		public string UnassignUser(long editorPersonaId, long userPersonaId)
		{
			ListResponse listResponse = new ListResponse();
			listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (listResponse.IsError) { return listResponse.ErrorReason; }

            
            string result = ChangeStatusAccountingUser(editorPersonaId, userPersonaId, false);
			if (result.Trim().ToUpper().Contains("INACTIVATED"))
			{
				UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);
				WriteToDiagnosticLog($"UnassignUser success userPersonaId:{userPersonaId}");

				// Activity Logging
				WriteUnassignActivityLog(editorPersonaId, userPersonaId);
			}
			else
			{
				WriteToDiagnosticLog($"UnassignUser failed userPersonaId:{userPersonaId}");
				return "Unassign failed. " + result;
			}

			return "";
		}

		#region Migration
		/// <summary>
		/// Get List of Accounting Users for Migration 
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
			var claimResposnse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
			if (claimResposnse.IsError) { response.ErrorReason = claimResposnse.ErrorReason; return response; }

            var userInfo = new Component.SharedObjects.Product.OneSiteAccounting.User[1];
			List<NameValuePair> loginInfo = new List<NameValuePair>
			{
				new NameValuePair { Name = "CompanyID", Value = _companyName },
				new NameValuePair { Name = "Login", Value = _intactLogin },
				new NameValuePair { Name = "Password", Value = _intactPassword },
				new NameValuePair { Name = "OneTimeExport", Value = "true" }
			};
			if (!string.IsNullOrEmpty(_productUserId))
			{
				loginInfo.Add(new NameValuePair { Name = "SystemIdentifier", Value = _productUserId });
			}
			userInfo[0] = new Component.SharedObjects.Product.OneSiteAccounting.User()
			{
				NameValuePair = loginInfo.ToArray()
			};

            var filter = true;
            var startRow = 0;
            var resultPerPage = 1000;
            if (datafilter != null)
            {
                if (datafilter.FilterBy.ContainsKey("filter"))
                {
                    filter = datafilter.FilterBy["filter"].ToUpper() == "NONMIGRATED";
                }
                if (datafilter.Pages != null)
                {
                    startRow = datafilter.Pages.StartRow;
                    resultPerPage = datafilter.Pages.ResultsPerPage;
                }
            }
            FilterSortParameters wsParams = new FilterSortParameters
            {
                StartPosition = startRow.ToString(),
                PageLength = resultPerPage.ToString(),
                FilterConditionList = new FilterConditionList[]
                {
                    new FilterConditionList(){
                        LogicalOperator = "OR",
                        FilterCondition = new FilterCondition[]
                        {
                            new FilterCondition()
                            {
                                PropertyName = "excludeassign",
                                ComparisionOperator = "equalto",
                                SearchValue = filter ? "1" : "0"
                            }
                        }
                    }
                }
            };
            WriteToDiagnosticLog("ManageProductOneSiteAccounting.GetAllUsers", new Dictionary<string, object> { { "user", RemovePrivateData(loginInfo.ToArray()) } });
			TotalRows[] results2 = new TotalRows[1];
			var users = _service.GetAllUsers(userInfo, wsParams, out results2);
			var totalRow = results2.FirstOrDefault() ?? new TotalRows() { TotalRows1 = "0" };
            
			if (users == null)
			{
				if (totalRow != null && totalRow.TotalRows1.ToUpper().Contains("NOT A VALID USERID"))
				{
					response.ErrorReason = "Invalid user.";
				}
				WriteToErrorLog($"ManageProductOneSiteAccounting.GetMigrationUsers- {response.ErrorReason} received from product for user with editorPersona id - {editorPersonaId}.");
				return response;
			}
			var migrationUsers = new List<MigrationUser>();
			foreach (var user in users)
			{
				var migrationUser = new MigrationUser();
				migrationUser.FirstName = user.FirstName;
				migrationUser.LastName = user.LastName;
				migrationUser.Username = user.UserID;
				migrationUser.UserId = user.UserID;
				migrationUser.Email = user.EmailAddress;
				migrationUser.CompanyInstanceSourceId = _companyName;
				migrationUser.Title = user.TITLE;
				migrationUser.MiddleName = user.MIDDLENAME;
				migrationUser.LastActivity = user.LASTACCESSDATE;
				migrationUser.Phone = user.PHONENUMBER;
				migrationUser.Status = user.USERSTATUS == "F" ? "Disabled" : "Active";
				migrationUsers.Add(migrationUser);
			}
			WriteToDiagnosticLog($"ManageProductOneSiteAccounting.GetUsers - Received users from product for user with editorPersona id - {editorPersonaId}.");
			response.RowsPerPage = Convert.ToInt32(wsParams.PageLength);
			response.ErrorReason = string.Empty;
			response.IsError = false;
			response.TotalPages = 1;
			response.Records = migrationUsers.Cast<object>().ToList();
            response.TotalRows = string.IsNullOrEmpty(totalRow.TotalRows1) ? 0 : Convert.ToInt32(totalRow.TotalRows1);
            return response;
		}

		/// <summary>
		/// Update the users migration status
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="migrateUsers"></param>
		/// <returns></returns>
		public MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers)
		{
			var migrateResponse = new MigrateResponse()
			{
				Status = false
			};

			var claimResposnse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
			if (claimResposnse.IsError) { migrateResponse.Message = claimResposnse.ErrorReason; return migrateResponse; }

			foreach (var migateUser in migrateUsers)
			{
				var loginInfo = new NameValuePair[4]
				{
					new NameValuePair { Name = "CompanyID", Value = _companyName },
					new NameValuePair { Name = "Login", Value = _intactLogin },
					new NameValuePair { Name = "Password", Value = _intactPassword },
					new NameValuePair { Name = "SystemIdentifier", Value = $"{_companyName}|{migateUser.UserId}" }
				};
				var message = "";
				if (migateUser.UsingUnifiedLogin)
				{
					WriteToDiagnosticLog("ManageProductOneSiteAccounting.UpdateUsersMigrationStatus.EnableGreenBookUser",
						new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo) } }
						);
					message += _service.EnableGreenBookUser(loginInfo);
					WriteToDiagnosticLog("ManageProductOneSiteAccounting.UpdateUsersMigrationStatus.EnableGreenBookUser",
					   new Dictionary<string, object>() { { "Message", message } }
					   );
				}
				else
				{
					WriteToDiagnosticLog("ManageProductOneSiteAccounting.UpdateUsersMigrationStatus.DisableGreenBookUser",
						new Dictionary<string, object>() { { "user", RemovePrivateData(loginInfo) } }
						);
					message += _service.DisableGreenBookUser(loginInfo);
					WriteToDiagnosticLog("ManageProductOneSiteAccounting.UpdateUsersMigrationStatus.DisableGreenBookUser",
						new Dictionary<string, object>() { { "Message", message } }
						);
				}
				migrateResponse.Message += message;
			}
			migrateResponse.Status = true;
			return migrateResponse;
		}

        #region User-Status

        /// <summary>
        /// Disables the Accounting Product user
        /// </summary>
        /// <param name="editorPersonaId">The editor persona identifier.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        /// <returns></returns>
        public bool ChangeUserStatus(long editorPersonaId, string userName, bool isActive = false)
        {
            ListResponse listResponse = new ListResponse();
            var result = "";
            Dictionary<string, object> logData = new Dictionary<string, object>();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (listResponse.IsError) { return false; }

            string companyInstanceSourceId = GetAccountingCompanyFromPersona(_editorPersona);
            if (string.IsNullOrEmpty(companyInstanceSourceId))
            {
                WriteToErrorLog(
                    $"ManageProductOneSiteAccounting.ChangeUserStatus - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}.");
                return false;
            }

            List<NameValuePair> parameters = new List<NameValuePair>{
                    new NameValuePair { Name = "CompanyID", Value = companyInstanceSourceId },
                    new NameValuePair { Name = "Login", Value = _intactLogin },
                    new NameValuePair { Name = "Password", Value = _intactPassword },
                    new NameValuePair { Name = "SystemIdentifier", Value = userName}
            };

            logData = new Dictionary<string, object>();
            logData.Add("parameters", RemovePrivateData(parameters.ToArray()));

            try
            {
                WriteToDiagnosticLog($"ManageProductOneSiteAccounting.ChangeUserStatus - Updating user status for user = {companyInstanceSourceId}|{userName}, isActive = {isActive}", logData);

                if (isActive)
                    result = _service.EnableUser(parameters.ToArray());
                else
                    result = _service.DisableUser(parameters.ToArray());

                if (result.Trim().ToUpper().Contains("INACTIVATED"))
                {
                    WriteToDiagnosticLog($"Enable/Disable success userName:{userName}");
                    return true;
                }
                else
                {
                    WriteToDiagnosticLog($"Enable/Disable failed userName:{userName}");
                    return false;
                }

            }
            catch (Exception ex)
            {
                WriteToErrorLog($"ManageProductOneSiteAccounting.ChangeUserStatus - Updating user status failed for user {companyInstanceSourceId}|{userName} by editorPersonaId = {editorPersonaId}", exception: ex);
                return false;
            }
        }

        #endregion

        #endregion


    }


    /// <summary>
    /// Used to build the XML required to call the OneSite web services
    /// </summary>
    public static class ManageProductOneSiteAccountingHelpers
	{
		/// <summary>
		/// Used to convert a OneSite Accounting property into a GreenBook property to be used by the UI
		/// </summary>
		/// <param name="properties">The list of properties to convert</param>
		/// <returns></returns>
		public static IList<ProductProperty> ToGBProperties(this LocationID[] properties)
		{
			if (properties == null) return null;
			IList<ProductProperty> results = new List<ProductProperty>();
			foreach (LocationID loc in properties)
			{
				results.Add(new ProductProperty
				{
					Name = loc.Name,
					ID = loc.LocationID1,
					Street1 = loc.Address1,
					Street2 = loc.Address2,
					City = loc.City,
					State = loc.State,
					Zip = loc.Zip,
					IsAssigned = (loc.Assigned.ToLower() == "true" ? true : false)
				});
			}
			return (from property in results orderby property.ID, property.Name select property).ToList();
		}

		/// <summary>
		/// Used to convert a OneSite Accounting property group into a GreenBook property group to be used by the UI
		/// </summary>
		/// <param name="propertyGroups">The list of propertyGroups to convert</param>
		/// <returns></returns>
		public static IList<ProductPropertyGroup> ToGBPropertyGroup(this LocationGroupID[] propertyGroups)
		{
			if (propertyGroups == null) return null;
			IList<ProductPropertyGroup> results = new List<ProductPropertyGroup>();
			foreach (LocationGroupID loc in propertyGroups)
			{
				results.Add(new ProductPropertyGroup
				{
					Name = loc.Name,
					ID = loc.ID,					
					IsAssigned = (loc.Assigned.ToLower() == "true" ? true : false),
					AssignedProperties = loc.Memberids.Split(',').ToList()
			});
			}
			return (from propertyGroup in results orderby propertyGroup.ID, propertyGroup.Name, propertyGroup.AssignedProperties select propertyGroup).ToList();
		}
		/// <summary>
		/// Used to convert a OneSite Accounting role into a GreenBook role to be used by the UI
		/// </summary>
		/// <param name="roles">The list of roles to convert</param>
		/// <returns></returns>
		public static IList<ProductRole> ToGBRoles(this RoleName[] roles)
		{
			if (roles == null) return null;
			IList<ProductRole> results = new List<ProductRole>();
			foreach (RoleName role in roles)
			{

				results.Add(new ProductRole
				{
					ID = role.Recordno,
					Name = role.Name,
					Description = role.Description,
					IsAssigned = (role.Assigned.ToLower() == "true" ? true : false)
				});
			}
			return (from role in results orderby role.Name select role).ToList();
		}

        /// <summary>
        /// Used to convert a OneSite Accounting right into a GreenBook right to be used by the UI
        /// </summary>
        /// <param name="permissions">The list of rights to convert</param>
        /// <returns></returns>
        public static IList<ProductRightAcct> ToRights(this PermissionID[] permissions)
        {
            if (permissions == null) return null;
            IList<ProductRightAcct> results = new List<ProductRightAcct>();
            int i = 1;
            char[] c = new char[1];
            c[0] = '|';
            foreach (PermissionID permission in permissions)
            {
                try
                {
                    if (permission.action.Trim().ToUpper() != "NONE")
                    {
                        results.Add(new ProductRightAcct
                        {
                            ID = i,//int.Parse(permission.rightID), // due to duplicate rightIds from Accounting
                            RightID = permission.rightID.Length == 0 ? 0 : int.Parse(permission.rightID), 
                            Alias = permission.right + " - " + permission.actionLabel,
                            CenterName = permission.application,
                            Description = permission.right + " - " + permission.actionLabel,
                            RolesAssigned = permission.roles.Trim().Length == 0 ? 0 : permission.roles.Split(c).Length,
                            Assigned = permission.value.ToUpper().Trim() == "TRUE" ? true : false,
							ModuleID = permission.moduleID.Length == 0 ? GetApplicationModuleID(permission.application) : permission.moduleID,
							Action = permission.action,
                            Right = permission.right,
                            ActionLabel = permission.actionLabel

                        }

                        );
                        i++;
                    }
                }
                catch { }
            }
            return results;
        }

        /// <summary>
		/// Used to convert a OneSite Accounting property into a GreenBook property to be used by the UI
		/// </summary>
		/// <param name="companies">The list of properties to convert</param>
		/// <returns></returns>
		public static List<ACCompany> ToGBCompanies(this CompanyID[] companies)
        {
            if (companies == null) return null;
            List<ACCompany> results = new List<ACCompany>();
            foreach (CompanyID cmp in companies)
            {
                results.Add(new ACCompany
                {
                   Id = cmp.CompanyID1,
                   Name = cmp.CompanyName,
                   isAssigned = cmp.Assigned == string.Empty ? false : bool.Parse(cmp.Assigned)
                });
            }
            return results;
        }

        /// <summary>
        /// Used to convert a OneSite Accounting property into a GreenBook property to be used by the UI
        /// </summary>
        /// <param name="enteties">The list of properties to convert</param>
        /// <returns></returns>
        public static List<ACProperty> ToGBEnteties(this EntityID[] enteties)
        {
            if (enteties == null) return null;
            List<ACProperty> results = new List<ACProperty>();
            foreach (EntityID loc in enteties)
            {
                results.Add(new ACProperty
                {
					Id = loc.EntityID1,
                    PropertyId = loc.EntityID1,
                    PropertyName = loc.EntityName,
                    CompanyId = loc.CompanyID,
                    CompanyName = loc.CompanyName,
                    IsAssigned = loc.Assigned == string.Empty ? false : bool.Parse(loc.Assigned),
                    MConsoleId = loc.MConsoleEntityID, 
                    IsCompanyAssigned = loc.Assigned == string.Empty  ? false :  (bool.Parse(loc.Assigned) == true && loc.EntityID1 == string.Empty) ? true : false

                });
            }
            return results;
        }


        /// <summary>
        /// Get the application moduleID
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        private static string GetApplicationModuleID(string application)
		{
			string moduleId = "";
			switch (application.Trim().ToUpper())
			{
				case "ACCOUNTS PAYABLE":
					moduleId = "3.AP";
					break;
				case "ACCOUNTS RECEIVABLE":
					moduleId = "4.AR";
					break;
				case "CASH MANAGEMENT":
					moduleId = "11.CM";
					break;
				case "ASSURANCE":
					moduleId = "19.AS";
					break;
				case "INTACCT-OPENAIR INTEGRATION":
					moduleId = "22.AIR";
					break;
				case "MY PRACTICE":
					moduleId = "13.PR";
					break;
				case "PAYROLL":
					moduleId = "20.CBS";
					break;               
                case "MY ACCOUNTING":
					moduleId = "14.ACCT";
					break;
				case "CONSOLE":
					moduleId = "15.CPA";
					break;
				case "401(K)":
					moduleId = "29.DEC";
					break;
				case "IMPORT EXPORT":
					moduleId = "32.IE";
					break;
				case "TIME & BILLING":
					moduleId = "23.TB";
					break;
				case "COMPANY":
					moduleId = "1.CO";
					break;
				case "CONSOLIDATION":
					moduleId = "10.CS";
					break;
				case "GENERAL LEDGER":
					moduleId = "2.GL";
					break;
				case "INVENTORY CONTROL":
					moduleId = "7.INV";
					break;
				case "INVENTORY":
					moduleId = "7.INV";
					break;
				case "PLATFORM SERVICES":
					moduleId = "39.CERP";
					break;
				case "PROJECTS":
					moduleId = "48.PROJACCT";
					break;
				case "PURCHASING":
					moduleId = "9.PO";
					break;
				case "REVENUE MANAGEMENT":
					moduleId = "54.REVREC";
					break;
				case "TIME & EXPENSES":
					moduleId = "6.EE";
					break;
				case "BUSINESS DEVELOPMENT":
					moduleId = "24.BD";
					break;
				case "CLIENT EXPENSES":
					moduleId = "21.CE";
					break;
				case "TAX EXPORT":
					moduleId = "17.TX";
					break;
				case "ORDER ENTRY":
					moduleId = "8.SO";
					break;
				case "MANAGEMENT CONSOLE":
					moduleId = "18.MPRAC";
					break;
				case "FINANCIAL ANALYSIS":
					moduleId = "30.LT";
					break;
				case "MY CLIENTS":
					moduleId = "16.CL";
					break;
				case "PAYROLL - ADP":
					moduleId = "31.ADP";
					break;
				case "WEB SERVICES":
					moduleId = "33.XDA";
					break;
				case "WRITEUP APPLICATION":
					moduleId = "34.WU";
					break;
				case "WRITE-UP":
					moduleId = "36.WUPACK";
					break;
				case "MULTI ENTITY CONSOLE":
					moduleId = "37.ME";
					break;
				case "INTACCT-SALESFORCE INTEGRATION":
					moduleId = "38.SFDC";
					break;
				case "PAYMENT SERVICES":
					moduleId = "40.CCP";
					break;
				case "WELLS FARGO PAYMENT MANAGER":
					moduleId = "42.WFPM";
					break;
				case "INTACCT-QUICKARROW INTEGRATION":
					moduleId = "44.QARROW";
					break;
				case "GLOBAL CONSOLIDATIONS":
					moduleId = "45.ATLAS";
					break;
				case "INTACCT-CLARIZEN INTEGRATION":
					moduleId = "49.CLARIZEN";
					break;
				case "AVALARA TAX":
					moduleId = "43.AVA";
					break;
				case "ONESITE ACCOUNTS RECEIVABLE":
					moduleId = "51.OAR";
					break;
				case "QUICKBOOKS MIGRATION":
					moduleId = "47.QB";
					break;
				case "INTACCT COLLABORATE":
					moduleId = "53.CHAT";
					break;
				case "EXTERNAL SERVICES PROVIDER":
					moduleId = "56.ESP";
					break;
				case "CONTRACT MODULE":
					moduleId = "55.CONTRACT";
					break;
				case "VENDOR PAYMENT SERVICES":
					moduleId = "52.OPYMTS";
					break;
				case "INTACCT-ZUORA INTEGRATION":
					moduleId = "50.ZUORA";
					break;
				case "DATA DELIVERY SERVICE":
					moduleId = "51.DDS";
					break;
				case "ADVANCED CRM INTEGRATION":
					moduleId = "61.SFDC2";
					break;
				case "PROPERTY MANAGEMENT":
					moduleId = "99.PM";
					break;
				case "REPORTING":
					moduleId = "59.CRW";
					break;
				case "DIGITAL BOARD BOOK":
					moduleId = "57.DBB";
					break;
				case "SPEND MANAGEMENT":
					moduleId = "58.SC";
					break;
				case "ADMINISTRATION":
					moduleId = "0.ADMIN";
					break;
				default:
					break;
			}
			return moduleId;
		}

		/// <summary>
		/// Used to convert a OneSite Accounting role into a GreenBook role to be used by the UI
		/// </summary>
		/// <param name="permissions">The list of roles to convert</param>
		/// <returns></returns>
		public static List<ProductRole> ToRoles(this PermissionID[] permissions)
		{
			if (permissions == null) return null;
			List<ProductRole> results = new List<ProductRole>();

			foreach (PermissionID permission in permissions)
			{
				try
				{
					roleDetails(ref results, permission);
				}
				catch (Exception ex)
				{
					var response = new ListResponse()
					{
						IsError = true,
						ErrorReason = ex.Message
					};
				}

			}
			return results;
		}

		/// <summary>
		/// Used to convert a OneSite Accounting role into a role list to be used by the UI
		/// </summary>
		/// <param name="permissions">The list of roles to convert</param>
		/// <returns></returns>
		public static List<ProductRole> ToRolesList(this PermissionuID[] permissions)
		{
			if (permissions == null) return null;
			List<ProductRole> results = new List<ProductRole>();
			int i = 1;
			foreach (PermissionuID permission in permissions)
			{
				try
				{
					ProductRole r = new ProductRole();
					r.IsAssigned = bool.Parse(permission.assigned);
					r.Name = permission.roleName;
					r.Roletype = "Custom";
					//r.ID = permission.roleID;
					r.ID = i.ToString();
					results.Add(r);
				}
				catch (Exception ex)
				{
					var response = new ListResponse()
					{
						IsError = true,
						ErrorReason = ex.Message
					};
				}
				i++;

			}
			return results;
		}



		public static void roleDetails(ref List<ProductRole> results, PermissionID permission)
		{

			char[] c = new char[1];
			c[0] = '|';
            
			if (!String.IsNullOrEmpty(permission.roles))
			{
				//string[] roles = permission.roles.Split(c);
                var roles = permission.roles.Split(c).Distinct(); // Accounting sending duplicate roles sometimes

                foreach (var item in roles)
				{
					string[] x = new string[1];
					x[0] = "@@";
					string[] nameId = item.Split(x, StringSplitOptions.None);
                   
					ProductRole pr = results.FirstOrDefault(p => int.Parse(p.ID) == int.Parse(nameId[0]));

					if (pr == null)
					{
						ProductRole r = new ProductRole();

						r.Name = nameId[1];
						r.RightsAssigned = permission.action.Trim().ToUpper() == "NONE" ? "0" : "1";
						r.Roletype = "Custom";
						r.ID = nameId[0];
						results.Add(r);
					}
					else
					{
						pr.RightsAssigned = (int.Parse(pr.RightsAssigned) + 1).ToString();
					}
				}
			}

		}

		/// <summary>
		/// Used to convert a OneSite Accounting applications  to be used by the UI
		/// </summary>
		/// <param name="applications">The list of applications to convert</param>
		/// <returns></returns>
		public static string[] ToCenters(this ApplicationID[] applications)
		{
			string[] results = new string[applications.Length];
			int i = 0;
			foreach (ApplicationID app in applications)
			{
				results.SetValue(app.Name, i);
				i++;
			}

			return results;
		}


		/// <summary>
		/// Used to convert a OneSite Accounting user into a GreenBook user to be used by the UI
		/// </summary>
		/// <param name="users">The list of users to convert</param>
		/// <returns></returns>
		public static IList<Component.SharedObjects.Product.ProductUser> ToGBUsers(this Component.SharedObjects.Product.OneSiteAccounting.User[] users)
		{
			if (users == null) return null;
			IList<Component.SharedObjects.Product.ProductUser> results = new List<Component.SharedObjects.Product.ProductUser>();
			return results;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="datafilter"></param>
		/// <param name="defaultFieldToSort"></param>
		/// <param name="start"></param>
		/// <param name="pageLength"></param>
		/// <param name="excludeAssigned"></param>
		/// <returns></returns>
		public static FilterSortParameters GenerateSearchAndPaging(RequestParameter datafilter, string defaultFieldToSort, int start, int pageLength, bool excludeAssigned = false)
		{
			//handle pagination
			FilterSortParameters wsParams = new FilterSortParameters { StartPosition = start.ToString(), PageLength = pageLength.ToString() };

			// nothing to filter
			if (datafilter == null) { datafilter = new RequestParameter(); }

			//handle sorting
			if (datafilter.SortBy.Count == 0)
			{
				datafilter = new RequestParameter() { SortBy = new Dictionary<string, string>() };
				datafilter.SortBy.Add("name", "asc");
			}

			SortConditionList sortList = new SortConditionList();
			if (datafilter.SortBy != null && datafilter.SortBy.Count > 0)
			{
				SortCondition[] scList = (from a in datafilter.SortBy.ToList() select new SortCondition { ColumnName = a.Key, SortDirection = a.Value }).ToArray();
				sortList.SortCondition = scList;
				wsParams.SortConditionList = new SortConditionList[] { sortList };
			}

			List<FilterCondition> filterCondition = new List<FilterCondition>();
			filterCondition.Add(new FilterCondition() { PropertyName = "excludeassign", ComparisionOperator = "equalto", SearchValue = (excludeAssigned == true ? "1" : "0") });
			FilterConditionList filterList = new FilterConditionList();
			if (datafilter.FilterBy != null && datafilter.FilterBy.Count > 0)
			{
				filterList.LogicalOperator = "OR";
				filterCondition.AddRange((from a in datafilter.FilterBy.ToList() select new FilterCondition { PropertyName = a.Key, SearchValue = a.Value, ComparisionOperator = "OR" }));
			}
			filterList.FilterCondition = filterCondition.ToArray();
			wsParams.FilterConditionList = new FilterConditionList[] { filterList };
			return wsParams;
		}

    }
}


using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.EmployeeAccess;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    public class ManageEmployeeAccess : ManageProductBase, IManageEmployeeAccess
    {
        /// <summary>
        /// User claim
        /// </summary>
        private DefaultUserClaim _userClaim;

        private IManageOrganization _manageOrganization;
        private IManageUser _manageUser;
        private IManagePersona _managePersona;
        private readonly IIntegrationTypeFactory _integrationTypeFactory;
        private readonly IManageProductOneSite _manageProductOneSite;
        readonly IManageUnifiedLogin _manageUnifiedLogin;
        private IManageProductUser _manageProductUser;
        #region Ctor


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userClaim"></param>
        public ManageEmployeeAccess(DefaultUserClaim userClaim) : base((int)ProductEnum.SupportTool, userClaim, null, null)
        {
            _productId = (int)ProductEnum.SupportTool;
            _userClaim = userClaim;
            _editorRealPageId = _userClaim.UserRealPageGuid;
            _blueBook = new ManageBlueBook(_userClaim);
            _userLoginRepository = new UserLoginRepository();
            _manageOrganization = new ManageOrganization(_userClaim);
            _manageUser = new ManageUser(_userClaim);
            _managePersona = new ManagePersona(_userClaim);
            _manageUnifiedLogin = new ManageUnifiedLogin(_userClaim);
            _manageProductOneSite = new ManageProductOneSite(_userClaim);
            _manageProductUser = new ManageProductUser(_userClaim);

            var manageProduct = new ManageProduct(_userClaim);
            var productInternalSettingRepository = new ProductInternalSettingRepository();
            _integrationTypeFactory = new IntegrationTypeFactory(manageProduct, _manageUnifiedLogin, _manageProductOneSite, _productRepository, productInternalSettingRepository, _userClaim);
        }
        #endregion

        #region Public Methods


        /// <summary>
        /// Returns all companies from GB
        /// </summary>
        public ListResponse GetCompanies(long editorPersonaId, string filter)
        {
            WriteToDiagnosticLog(
                $"EmployeeAccess - ManageEmployeeAccess.GetCompanies at beginning of method for user with editorPersona id - {editorPersonaId}");

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog(
                        $"EmployeeAccess - ManageEmployeeAccess.GetCompanies.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                    return result;
                }

                // get companies from DB for EmployeeAccess 
                WriteToDiagnosticLog(
                   $"EmployeeAccess - Getting all GB companies from GB DB - pr.ListCompanies with filter- {filter}");

                UnifiedLoginRepository umr = new UnifiedLoginRepository();

                List<UnifiedLoginCompany> gbAllCompanies = umr.ListCompanies(filter);
                List<UnifiedLoginCompany> gbAllActiveCompanies = gbAllCompanies?.Where(c => c.IsActive == true).ToList();

                // Get BooksCompanyMasterIds - RPUP id
                //string comIdsRpUp = GetCompanyIds(gbAllCompanies);
                WriteToDiagnosticLog(
                    $"EmployeeAccess - ManageEmployeeAccess.Getcompanies.GetCompanyIds() completed for user with editorPersona id - {editorPersonaId}");

                IList<Company> bbCompanies = _blueBook.GetCompanyListByCompIds(gbAllActiveCompanies);

                List<CompanyDetails> mergedCompanies = MergeCompanies(gbAllActiveCompanies, bbCompanies);

                WriteToDiagnosticLog(
                    $"EmployeeAccess - ManageEmployeeAccess.Getcompanies.GetCompanyListByCompIds() completed for user with editorPersona id - {editorPersonaId}");

                response = new ListResponse()
                {
                    Records = mergedCompanies.Cast<object>().ToList(),
                    TotalRows = mergedCompanies.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = $"EmployeeAccess - There was a problem getting the companies.";
                WriteToErrorLog($"EmployeeAccess - ManageEmployeeAccess.Getcompanies Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Returns all Users from GB
        /// </summary>
        public ListResponse GetUsers(long editorPersonaId, string filter)
        {
            WriteToDiagnosticLog(
                $"EmployeeAccess - ManageEmployeeAccess.GetUsers at beginning of method for user with editorPersona id - {editorPersonaId}");

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog(
                        $"EmployeeAccess - ManageEmployeeAccess.GetUsers.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                    return result;
                }

                // get companies from DB for EmployeeAccess 
                WriteToDiagnosticLog(
                   $"EmployeeAccess - Getting all GB users from GB DB - pr.ListCompanies with filter- {filter}");

                UnifiedLoginRepository umr = new UnifiedLoginRepository();

                List<UnifiedLoginCompany> gbAllCompanies = umr.ListCompanies();

                List<UserDetail> ulUsersByFilter = umr.ListUsers(filter);
                if (ulUsersByFilter != null && ulUsersByFilter.Count > 0)
                {
                    foreach (var item in ulUsersByFilter)
                    {
                        if (item.Name3rdPartyIDP.ToUpper() == "IDENTITYSERVER")
                        {
                            item.Name3rdPartyIDP = "None";
                        }
                    }
                }

                List<UserDetail> mergedUserCompanies = MergeUserCompanies(gbAllCompanies, ulUsersByFilter);

                WriteToDiagnosticLog(
                    $"EmployeeAccess - ManageEmployeeAccess.GetUsers completed for user with editorPersona id - {editorPersonaId}");

                response = new ListResponse()
                {
                    Records = mergedUserCompanies.Cast<object>().ToList(),
                    TotalRows = mergedUserCompanies.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = $"EmployeeAccess - There was a problem getting the users.";
                WriteToErrorLog($"EmployeeAccess - ManageEmployeeAccess.GetUsers Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
            }

            return response;
        }


        /// <summary>
        /// Gets personaId if existed else, creates and gets one
        /// </summary>
        public EmployeePersona GetOrCreateEmployeePersonaId(Guid companyRealPageId, string loginName) 
        {
            EmployeePersona employeePersona = new EmployeePersona();
            employeePersona.RealpageUserId = _userClaim.UserRealPageGuid;

            var userPersonaOrganizationList = _userLoginRepository.ListOrganizationByLoginName(loginName);
            
            if (userPersonaOrganizationList != null && userPersonaOrganizationList.Count > 0)
            {
                var user = userPersonaOrganizationList.Where(x => x.OrganizationRealPageId == companyRealPageId).FirstOrDefault();
                if (user != null)
                {
                    employeePersona.PersonaId = user.PersonaId;
                }
                else
                {
                    employeePersona.PersonaId = CreatePersonaInCompany(loginName, companyRealPageId);
                }
            }

            return employeePersona;
            
        }

        public string CreateEmployeeProductUser(int productId, long personaId)
        {
            long adminUserPersonaId = 0;
            var adminCreatorRealPageId = Guid.Empty;
            var userPersona = _managePersona.GetPersona(personaId);

            if (userPersona.Organization.RealPageId != Guid.Empty)
            {
                adminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(userPersona.Organization.RealPageId);
                if (adminCreatorRealPageId == Guid.Empty)
                {
                    return "Missing company admin user.";
                }
                //recreate clams
                adminUserPersonaId = _managePersona.GetFirstAvailablePersonaByCompany(adminCreatorRealPageId, userPersona.OrganizationPartyId).PersonaId;
                _manageProductUser = new ManageProductUser(_userClaim);
            }

            var rolePropertyList = new RolePropertyList();
            // the following needs refinement
            var productADGroupRoleList = _productRepository.GetADGroupProductRoleByProductId(productId);
            if (productADGroupRoleList.Count > 0)
            {
                rolePropertyList.RoleList = new List<string>();
                foreach (var productRole in productADGroupRoleList)
                {
                    rolePropertyList.RoleList.Add(productRole.RoleName);
                }
            }

            rolePropertyList.PropertyList = new List<string>() { "-1" };
            // the following needs refinement

            var productUser = new ProductUserProperitiesRoles()
            {
                RealPageId = adminCreatorRealPageId,
                ProductId = productId,
                CreateUserPersonaId = adminUserPersonaId,
                AssignUserPersonaId = personaId,
                CorrelationId = _userClaim.CorrelationId,
                InputJson = JsonConvert.SerializeObject(rolePropertyList)
            };
            
            return _manageProductUser.CreateProductUser(productUser);
        }

        #endregion

        #region Private Methods  

        private List<CompanyDetails> MergeCompanies(List<UnifiedLoginCompany> gbcompanies, IList<Company> bbcompanies)
        {
            List<CompanyDetails> compList = new List<CompanyDetails>();
            foreach (var gb in gbcompanies)
            {
                CompanyDetails cd = new CompanyDetails();
                Company c = bbcompanies.FirstOrDefault((p => p.CustomerCompanyId == gb.BooksCustomerMasterId));
                cd.CompanyName = gb.CompanyName;
                cd.CompanyRealPageId = gb.CompanyRealPageId;
                cd.UserRealPageId = gb.UserRealPageId;
                cd.UserLoginAs = gb.UserLoginAs;
                cd.PartyId = gb.PartyId;
                cd.IsActive = gb.IsActive;

                if (c != null)
                {
                    cd.CompanyId = c.CustomerCompanyId;
                    cd.PhoneNumber = c.PhoneNumber;

                    if (c.CustomerCompanyLocation != null)
                    {
                        foreach (var comp in c.CustomerCompanyLocation)
                        {
                            if (comp.IsPrimary == true)
                            {
                                cd.Address = comp.Address;
                                cd.City = comp.City;
                                cd.Country = comp.Country;
                                cd.County = comp.County;
                                cd.State = comp.State;
                                cd.PostalCode = comp.PostalCode;
                            }
                        }
                    }
                    compList.Add(cd);
                }
                else
                {
                    if (gb.BooksCustomerMasterId == -2)
                    {
                        cd.Address = "REALPAGE INTERNAL USE ONLY!";
                        compList.Add(cd);
                    }
                }
            }
            return compList;
        }

        private List<UserDetail> MergeUserCompanies(List<UnifiedLoginCompany> gbcompanies, List<UserDetail> ulusers)
        {
            List<CompanyDetails> compList = new List<CompanyDetails>();
            foreach (var gb in gbcompanies)
            {
                CompanyDetails cd = new CompanyDetails();
                foreach (var bb in ulusers)
                {
                    if (gb.PartyId == bb.CompanyId)
                    {
                        bb.CompanyRealPageId = gb.CompanyRealPageId;
                        bb.UserRealPageId = gb.UserRealPageId;
                        bb.BooksMasterId = gb.CompanyId;
                    }
                }
            }
            return ulusers;
        }
        
        private string GetCompanyIds(List<UnifiedLoginCompany> companies)
        {
            string compIds = "";
            foreach (var item in companies)
            {
                if (item.BooksCustomerMasterId > 0)
                {
                    if (compIds == "")
                    {
                        compIds = item.BooksCustomerMasterId.ToString();
                    }

                    compIds += "," + item.BooksCustomerMasterId;
                }
            }

            return compIds;
        }

        private long CreatePersonaInCompany(string loginName, Guid companyRealPageId) 
        {
            long persona = 0;

            var newProfile = CreateNewProfile(companyRealPageId);
            var newUser = _manageUser.CreateUser(newProfile, newProfile.Persona);
            persona = newUser.PersonaId;

            return persona;
        }

        private ProfileDetail CreateNewProfile(Guid companyRealPageId)
        {
            
            ProfileDetail newProfile = new ProfileDetail();
            newProfile.FirstName = _userClaim.FirstName; 
            newProfile.LastName = _userClaim.LastName; 
            newProfile.CreateUserSourceType = CreateUserSourceType.UnifiedPlatform;

            IList<Persona> personaList = new List<Persona>();
            Persona persona = new Persona();
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();
            
            IList<PersonaEnvironment> personaEnvironment = _managePersona.GetPersonaEnvironmentType();
            var personaEnv = personaEnvironment.SingleOrDefault<PersonaEnvironment>(p => p.Name == "Production");
            
            persona.Name = newProfile.UserTypeId == (int)UserRoleType.SuperUser ? "System Administrator" : "Primary";
            persona.PersonaEnvironmentTypeId = (int)personaEnv.PersonaEnvironmentTypeId;
            persona.FromDate = utcNow.AddMinutes(-5);
            persona.ThruDate = null;
            personaList.Add(persona);
            newProfile.Persona = personaList;

            var org = _manageOrganization.GetOrganization(companyRealPageId);
            newProfile.organization.Add(org);
            
            newProfile.UserTypeId = 405;

            UserLogin ul = new UserLogin();
            ul.LoginName = _userClaim.LoginName; 
            ul.IsActive = true;
            ul.IsPending = false;
            ul.IsExpired = false;
            ul.FromDate = DateTime.UtcNow.AddMinutes(-5);
            ul.Is3rdPartyIDP = false;

            newProfile.userLogin = ul;

            List<ProductBatch> pbs = new List<ProductBatch>();
            ProductBatch pb = new ProductBatch();
            pb.ProductId = 3;
            pb.StatusTypeId = 5;
            pb.RetryCount = 0;

            RolePropertyList rpl = new RolePropertyList();
            rpl.IsVendorRecommendationChanges = false;
            rpl.IsInsuranceExpired = false;
            rpl.IsVendorNotLinkedToAnyProperty = false;

            Notifications notification = new Notifications() { 
                amenitiesViaEmail = false, managerFdiViaEmail = false, managerMrViaEmail = false 
            };

            rpl.Notifications = notification;

            rpl.CanReceiveMonthlyReport = false;
            rpl.IsAssignedNewPropertyByDefault = false;
            rpl.UsePrimaryProperties = false;
            rpl.HasAccessToAllCurrentFutureProperties = false;
            rpl.HasAccessToSiteSpendManagementOnly = false;

            var integration = _integrationTypeFactory.GetIntegration(pb.ProductId);
            ListResponse result = integration.GetRoles(_userClaim.PersonaId, 0, org.PartyId, null, null);

            List<UnifiedLoginRoleRights> roleRights = new List<UnifiedLoginRoleRights>();
            
            foreach (var item in result.Records)
                roleRights.Add((UnifiedLoginRoleRights)item);

            var role = roleRights.Where(x => x.Role == "User Administrator" && x.Roletype == "System").FirstOrDefault();

            if (role != null)
                rpl.RoleList = new List<string>() { role.RoleId.ToString() };

            rpl.PropertyList = new List<string>() { "-1" };
            

            pb.InputJson = rpl;

            pbs.Add(pb);

            newProfile.productBatch = pbs;

            return newProfile;
        }
        #endregion
    }





}

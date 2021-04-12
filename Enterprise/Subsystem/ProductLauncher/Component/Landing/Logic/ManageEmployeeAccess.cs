using System;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.EmployeeAccess;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    public class ManageEmployeeAccess : ManageProductBase, IManageEmployeeAccess
    {
        /// <summary>
        /// User claim
        /// </summary>
        private DefaultUserClaim _userClaim;

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

                List<UnifiedLoginCompany> gbAllActiveCompanies = (umr.ListCompanies(filter))?.Where(c => c.IsActive == true).ToList();

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

        #endregion
    }





}

using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model.SeniorLeadManagement;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    internal sealed class ManageProductSeniorLeadManagment : ManageProductBase
    {
        #region "Attributes"

        private DefaultUserClaim _userClaims;

        private ProductEnum ProductType { get; set; }

        private long EditorPersonaId { get; set; }
        private long UserPersonaId { get; set; }

        #endregion

        #region "Constructors"

        public ManageProductSeniorLeadManagment(DefaultUserClaim userClaims) : base((int)ProductEnum.SeniorLeadManagement, userClaims, null)
        {
            ProductType = ProductEnum.SeniorLeadManagement;
            _userClaims = userClaims;
            _editorRealPageId = userClaims.UserRealPageGuid;
            _blueBook = new ManageBlueBook(userClaims);

            //_seniorLeadManagementUrl = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
            //_username = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.ToUpper() == "APIUSERNAME").Value));
            //_password = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.ToUpper() == "APIPASSWORD").Value));

            //_mtApiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTAPIENDPOINT").Value;
            //_mtTokenEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTTOKENENDPOINT").Value;
            //_mtClientId = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTCLIENTID").Value;
            //_mtClientSecret = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTCLIENTSECRET").Value;

            //_service.Url = _onesiteUrl;
            //_service.PreAuthenticate = true;
            //_service.Credentials = new System.Net.NetworkCredential(_username, _password);
        }

        #endregion

        #region "Internal Methods"

        internal string ManageSeniorLeadManagementUser(long editorPersonaId, long userPersonaId, RolePropertyList rolePropertyList, UserDetails subjectUserDetails, bool isUserProfileChanged = false)
        {
            string result = string.Empty;
            this.EditorPersonaId = editorPersonaId;
            this.UserPersonaId = userPersonaId;

            WriteToDiagnosticLog(
              $"ManageProductInvokerBase.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorPersonaId}. At beginning of method.");

            // Get product user object 
            var productUser = new IntegrationProductUser
            {
                UserId = string.IsNullOrEmpty(subjectUserDetails.ProductUserId) ? "0" : subjectUserDetails.ProductUserId,
                // TODO: LoginName = string.IsNullOrEmpty(subjectUserDetails.LoginName) ? subjectUserDetails.LoginName : GetUniqueProductLogin(subjectUserDetails.LoginName),
                // TODO: CompanyId = CompanyInstanceSourceId,
                FirstName = subjectUserDetails.FirstName,
                LastName = subjectUserDetails.LastName,
                Email = subjectUserDetails.Email,
                Phone = subjectUserDetails.PhoneNumber,
                IsActive = true,
                IsMigratedUser = true,
                PropertyGroups = (rolePropertyList.PropertyGroupList == null) ? new List<string>() : rolePropertyList.PropertyGroupList,
                Properties = rolePropertyList.PropertyList,
                Roles = rolePropertyList.RoleList?.ConvertAll<string>(x => x.ToString()),
                PropertyRoles = rolePropertyList.PropertyRoleList,
                OrganizationRoles = rolePropertyList.OrganizationRoleList,
                CanReceiveMonthlyReport = rolePropertyList.CanReceiveMonthlyReport,
                PhoneNumbers = subjectUserDetails.PhoneNumbers,
                OneSiteUserInfo = GetOneSiteUserInfo(rolePropertyList)
            };

            if (subjectUserDetails.UserRoleTypeId == (int)UserRoleType.SuperUser)
            {
                productUser.IsAdminUser = true;
            }

            if (string.IsNullOrEmpty(subjectUserDetails.ProductUserName))
            {
                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorPersonaId}. Calling CreateUser.");

                // Get User & check if already exist 
                bool isUserExistInProduct = false;
                // TODO: isUserExistInProduct = CheckUserExistInProduct(productUser.LoginName);
                if (isUserExistInProduct)
                {
                    WriteToErrorLog(
                        $"ManageProductInvokerBase.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorPersonaId}. Product User {productUser.LoginName} already exist.");

                    return $"{productUser.LoginName} already exist in the product {ProductType}.";
                }

                // Create User
                // TODO: result = CreateUser(productUser);
            }
            else
            {
                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorPersonaId}. Calling UpdateUser.");
                // Update user with Id/Login from product
                productUser.UserId = subjectUserDetails.ProductUserId;
                productUser.LoginName = subjectUserDetails.ProductUserName;

                // TODO:  result = UpdateUser(productUser, batchProcessType);
            }
            return result;
        }

        #endregion

        #region  "Private Methods"

        private OneSiteUserInfo GetOneSiteUserInfo(RolePropertyList rolePropertyList)
        {
            OneSiteUserInfo oneSiteUserInfo = new OneSiteUserInfo();

            return oneSiteUserInfo;
        }

        private string CreateUser(IntegrationProductUser productUser)
        {
            //WriteToDiagnosticLog(
            //    $"ManageProductInvokerBase.CreateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

            //var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PostUserEndpoint);

            //WriteToDiagnosticLog(
            //    $"ManageProductInvokerBase.CreateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}.");

            //// dump api info
            //DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, productUser);

            //var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
            //var result = integration.PostEntity<IntegrationProductUser>(productUser);

            //if (result.IsSuccessStatusCode)
            //{
            //    WriteToDiagnosticLog(
            //        $"ManageProductInvokerBase.CreateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received success. Updating Geenbook mapping.");

            //    // map product user in green book
            //    _dataCollector.CreateProductUserInGreenBook(SubjectUserDetails.PersonaId, result.Content, ProductId, productUser.LoginName);

            //    // OPTIONAL - If product needs more attributes than userid or loginName then override in the product (e.g. PAM uses)
            //    CreateAdditionalSamlUserAttribute(SubjectUserDetails.PersonaId, ProductId, productUser);

            //    // activity logging
            //    ProductActivityLogger.WriteCreateUserActivityLog(EditorUserDetails, SubjectUserDetails, BlueBookGbProductMap.Name, BlueBookGbProductMap.BooksProductCode,
            //        CorrelationId);

            //    return string.Empty;
            //}

            //WriteToErrorLog(
            //    $"ManageProductInvokerBase.CreateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. result received - {result}.");

            //return result.Content;

            return string.Empty;
        }

        #endregion
    }
}

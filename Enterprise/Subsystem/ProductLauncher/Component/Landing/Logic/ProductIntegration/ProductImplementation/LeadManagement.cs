using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.ProductImplementation
{
    /// <summary>
    /// Product specific implementation 
    /// Override methods from base class if custom implementation
    /// </summary>
    public sealed class LeadManagement : ManageProductInvokerBase, IManageProductIntegration
    {
        #region Ctor

        public LeadManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims) : base(productType, editorPersonaId, subjectPersonaId, userClaims)
        { }

        public LeadManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims, IDataCollector injectedDataCollector, IManagePersona injectedManagePersona, IProductInternalSettingRepository productInternalSettingRepository) :
            base(productType, editorPersonaId, subjectPersonaId, userClaims, injectedDataCollector, injectedManagePersona, productInternalSettingRepository)
        { }

        #endregion

        protected override void ApplySuperUserData(IntegrationProductUser productUser)
        {
            // super user related assignments
            if (ProductType == ProductEnum.LeadAnalytics)
            {
                productUser.Roles = new List<string> { "18" };
            }
            else
            {
                productUser.Roles = new List<string> { "17" };
            }

            productUser.Properties = new List<string>() { "all" };
            productUser.PropertyGroups = new List<string>();
        }

        protected override void UpdateSamlUserAttribute(long personaId, int productId,
            string productUserId, string productUserLoginName, string productUserEmail)
        {
            WriteToDiagnosticLog(
                $"LeadManagement.UpdateSamlUserAttribute - Product {ProductType} productUserLoginName - {productUserLoginName}. At beginning of the method.");

            // Issue - GB-4715
            // if userName not matches with email then update user login with email
            if (!productUserLoginName.Equals(productUserEmail, StringComparison.OrdinalIgnoreCase))
            {
                _dataCollector.UpdateSamlUserAttribute(personaId, productId, SamlAttributeEnum.productUsername, productUserEmail);
            }
        }

        /// <summary>
		/// Create or update product user
		/// Gets called from Product-Batch
		/// </summary> 
        public override string CreateUpdateProductUser(ProductUserRolePropertiesGroups userRolePropertiesRegion, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            string result;

            WriteToDiagnosticLog($"LeadManagement.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of method.");

            // Get product user object 
            var newProductUser = GenerateProductUserObject(userRolePropertiesRegion);

            if (SubjectUserDetails.UserRoleTypeId == (int)UserRoleType.UserNoEmail)
            {
                newProductUser.LoginName = newProductUser.Email;
            }

            var productUser = getBaseUserDataFromProduct(newProductUser.LoginName);
            //For Multi company user creation first check user data from product,if user data exists then do put else post

            if (string.IsNullOrEmpty(SubjectUserDetails.ProductUserName) && productUser == null)
            {
                WriteToDiagnosticLog($"LeadManagement.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling CreateUser.");
                // Create User
                result = CreateUser(newProductUser);
            }
            //Create Multi company with put
            else if (string.IsNullOrEmpty(SubjectUserDetails.ProductUserName) && productUser != null)
            {
                result = CreateMultiCompanyUser(productUser);
            }
            else
            {
                WriteToDiagnosticLog($"LeadManagement.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling UpdateUser.");
                // Update user with Id/Login from product
                if (productUser != null)
                {
                    newProductUser.UserId = productUser.UserId;
                    newProductUser.LoginName = productUser.LoginName;
                }
                else
                {
                    newProductUser.UserId = SubjectUserDetails.ProductUserId;
                    newProductUser.LoginName = SubjectUserDetails.ProductUserName;
                }

                result = UpdateUser(newProductUser, batchProcessType);
            }
            return result;
        }

        /// <summary>
		/// Create a user in the product
		/// </summary>
        /// 
        private string CreateMultiCompanyUser(IntegrationProductUser productUser)
        {
            WriteToDiagnosticLog(
                $"ManageProductInvokerBase.CreateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

            var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PostUserEndpoint);

            WriteToDiagnosticLog(
                $"ManageProductInvokerBase.CreateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}.");

            // dump api info
            DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, productUser);

            var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
            var result = integration.PutEntity<IntegrationProductUser>(productUser);

            if (result.IsSuccessStatusCode)
            {
                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.CreateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received success. Updating Geenbook mapping.");

                // map product user in green book
                _dataCollector.CreateProductUserInGreenBook(SubjectUserDetails.PersonaId, result.Content, ProductId, productUser.LoginName);

                // OPTIONAL - If product needs more attributes than userid or loginName then override in the product (e.g. PAM uses)
                CreateAdditionalSamlUserAttribute(SubjectUserDetails.PersonaId, ProductId, productUser);

                // activity logging
                ProductActivityLogger.WriteCreateUserActivityLog(EditorUserDetails, SubjectUserDetails, BlueBookGbProductMap.Name, BlueBookGbProductMap.BooksProductCode,
                    CorrelationId);

                return string.Empty;
            }

            WriteToErrorLog(
                $"ManageProductInvokerBase.CreateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. result received - {result}.");

            return result.Content;
        }

        #region private
        private IntegrationProductUser getBaseUserDataFromProduct(string loginNameToCheck, string baseUrlAndQuery = null)
        {
            if (string.IsNullOrEmpty(baseUrlAndQuery))
                baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);

            bool isCompanyIdRequiredToQuery = baseUrlAndQuery.Contains("{0}");
            if (isCompanyIdRequiredToQuery)
            {
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, loginNameToCheck);
            }
            else
            {
                baseUrlAndQuery = string.Format(baseUrlAndQuery, loginNameToCheck);
            }
            WriteToDiagnosticLog(
              $"ManageProductInvokerBase.GetProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

            var productUser = GetResultFromApi<IntegrationProductUser>(baseUrlAndQuery, false);

            WriteToDiagnosticLog(
                $"ManageProductInvokerBase.GetProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}.");

            return productUser;
        }
        #endregion
    }
}
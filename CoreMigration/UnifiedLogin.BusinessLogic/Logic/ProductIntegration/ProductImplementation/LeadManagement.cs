using System;
using System.Collections.Generic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.ProductImplementation
{
    /// <summary>
    /// Product specific implementation 
    /// Override methods from base class if custom implementation
    /// </summary>
    public sealed class LeadManagement : StandardV1ProductIntegration, IManageProductIntegration
    {
        #region Ctor

        public LeadManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims) : base((int)productType, editorPersonaId, subjectPersonaId, userClaims)
        { }

        public LeadManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims, IDataCollector injectedDataCollector, IManagePersona injectedManagePersona, IProductInternalSettingRepository productInternalSettingRepository) :
            base((int)productType, editorPersonaId, subjectPersonaId, userClaims, injectedDataCollector, injectedManagePersona, productInternalSettingRepository)
        { }

        #endregion

        protected override void ApplySuperUserData(IntegrationProductUser productUser)
        {
            // super user related assignments
            if (ProductId == (int)ProductEnum.LeadAnalytics)
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
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateSamlUserAttribute", $"Product {ProductId} productUserLoginName - {productUserLoginName}. At beginning of the method." });

            // Issue - GB-4715
            // if userName not matches with email then update user login with email
            if (!productUserLoginName.Equals(productUserEmail, StringComparison.OrdinalIgnoreCase))
            {
                _dataCollector.UpdateSamlUserAttribute(personaId, productId, SamlAttributeEnum.productUsername, productUserEmail);
            }
        }
        public override string CreateUpdateProductUser(ProductUserRolePropertiesGroups userRolePropertiesRegion, out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            string result = string.Empty;
            additionalParameters = new List<AdditionalParameters>();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateProductUser", $"editorPersona id - {EditorUserDetails.PersonaId}. At beginning of method." });
            // Get product user object 
            var newProductUser = GenerateProductUserObject(userRolePropertiesRegion);

            if (string.IsNullOrEmpty(SubjectUserDetails.ProductUserName))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateProductUser", $"editorPersona id - {EditorUserDetails.PersonaId}. Calling CreateUser." });
                if (!CheckUserExistInProduct(newProductUser.LoginName))
                {
                    newProductUser.LoginName = $"{newProductUser.FirstName.TrimWhiteSpace().Substring(0, 1)}" + $"{newProductUser.LastName.TrimWhiteSpace()}".ToLower() + "_"+ _productDetails.BooksProductCode + "_" + SubjectUserDetails.PersonaId;
                    result = CreateUser(newProductUser, out additionalParameters);
                }
            }
            else
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling UpdateUser." });
                // Update user with Id/Login from product
                newProductUser.UserId = SubjectUserDetails.ProductUserId;
                newProductUser.LoginName = SubjectUserDetails.ProductUserName;

                result = UpdateUser(newProductUser, batchProcessType, out additionalParameters);
            }
            return result;
        }
    }
}
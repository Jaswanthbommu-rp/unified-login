using System.Collections.Generic;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers
{
    public interface IDataCollector
    {
        void CreateProductUserInGreenBook(long subjectPersonaId, dynamic userResult, int productId, IntegrationProductUser productUser);
        void UpdateProductUserInGreenBook(long subjectPersonaId, dynamic userResult, int productId, IntegrationProductUser productUser);

        GbProductMap GetBlueBookProductMap(int productId);
        CustomerCompanyMap GetProductCompanyMap(string blueBookProductCode, int booksMasterId, DefaultUserClaim userClaims, string domain);
        UserDetails GetUserDetailsByPersona(long personaId, int productId);
        void UpdateProductSettingProductStatus<T>(long subjectPersonaId, string settingType, int productId, T value);
        void UpdateSamlUserAttribute(long personaId, int productId, SamlAttributeEnum attributeType, string newValue);
        void CreateSamlUserAttribute(long subjectPersonaId, int productId, SamlAttributeEnum samlAttributeEnum, string value);
        AdUserDetail GetAzureUserDetails(long userId);
        void AddUpdateEmployeeProductADGroupMapping(long personaId, int productId, int adGroupId);
        IList<EmployeeProductMapping> GetEmployeeProductADGroupMapping(long personaId, int productId);
    }
}

using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers
{
    public interface IDataCollector
    {
        void CreateProductUserInGreenBook(long subjectPersonaId, dynamic userResult, int productId, string productLoginName);

        GbProductMap GetBlueBookProductMap(int productId);
        CustomerCompanyMap GetProductCompanyMap(string blueBookProductCode, int booksMasterId, DefaultUserClaim userClaims, string domain);
        UserDetails GetUserDetailsByPersona(long personaId, int productId);
        void UpdateProductSettingProductStatus<T>(long subjectPersonaId, string settingType, int productId, T value);
        void UpdateSamlUserAttribute(long personaId, int productId, SamlAttributeEnum attributeType, string newValue);
        void CreateSamlUserAttribute(long subjectPersonaId, int productId, SamlAttributeEnum samlAttributeEnum, string value);
        AdUserDetail GetAzureUserDetails(long userId);
        void AddUpdateEmployeeProductADGroupMapping(long personaId, int productId, int adGroupId);
    }
}

using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
    /// <summary>
    /// Used to get employee information from Microsoft Azure
    /// </summary>
    public interface IManageMicrosoftAzure
    {
        /// <summary>
        /// Used to get employee information for the given user name
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        AzureUser GetADUserInfo(string userName);

    }
}

using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
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

using System.ComponentModel.DataAnnotations;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper
{
    /// <summary>
    /// 
    /// </summary>
    public class EmailFormatValidation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsValidEmail(string email)
        {
            return (new EmailAddressAttribute().IsValid(email));
            /*
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
            */
        }
    }
}

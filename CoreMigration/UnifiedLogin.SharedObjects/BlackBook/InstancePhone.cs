namespace UnifiedLogin.SharedObjects.BlackBook
{
    /// <summary>
    /// Used to store phone numbers for the given property
    /// </summary>
    public class InstancePhone
    {
        /// <summary>
        /// The phone number to store
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// The type of phone number being stored
        /// </summary>
        public string PhoneType { get; set; }
    }
}

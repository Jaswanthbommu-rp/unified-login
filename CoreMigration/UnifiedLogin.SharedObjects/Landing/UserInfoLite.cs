namespace UnifiedLogin.SharedObjects.Landing
{
    public class UserInfoLite
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LoginName { get; set; }
        public long UserId { get; set; }
        public long SuperVisorUserId { get; set; }
        public bool IsReadOnly { get; set; }
        public long OrganizationPartyId { get; set; }
    }
}

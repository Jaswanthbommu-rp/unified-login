namespace UnifiedLogin.SharedObjects.BlackBook
{
    /// <summary>
    /// 
    /// </summary>
    public class CompanyPropertyInstanceMap
    {
        public string Id { get; set; }
        public int CompanyInstanceId { get; set; }
        public int PropertyInstanceId { get; set; }
        public string companyRelationship { get; set; }
        public string Source { get; set; }
        public string CreatedBy { get; set; }
        public PropertyInstance[] PropertyInstance { get; set; }
    }
}

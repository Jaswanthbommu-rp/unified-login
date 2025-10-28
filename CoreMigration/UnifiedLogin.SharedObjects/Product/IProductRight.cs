namespace UnifiedLogin.SharedObjects.Product
{
    public interface IProductRight
    {
        int ID { get; set; }
        /// <summary>
        /// The description of the right
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string CenterName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        bool Assigned { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int RolesAssigned { get; set; }
    }
}
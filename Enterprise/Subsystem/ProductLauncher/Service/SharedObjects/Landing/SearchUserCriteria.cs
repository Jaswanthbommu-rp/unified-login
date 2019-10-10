using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    /// <summary>
    /// 
    /// </summary>
    public class SearchUserCriteria
    {
        /// <summary>
        /// 
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PersonType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SearchUserSortOrderType SortOrder { get; set; }
    }
}

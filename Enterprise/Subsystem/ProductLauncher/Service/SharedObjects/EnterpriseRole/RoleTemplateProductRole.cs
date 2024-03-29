using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole
{
	public class RoleTemplateProductRole
	{
		/// <summary>
		/// RoleTemplateId
		/// </summary>
		public int RoleTemplateId { get; set; }

		/// <summary>
		/// PartyId
		/// </summary>
		public int PartyId { get; set; }

		/// <summary>
		/// RoleTemplateName
		/// </summary>
		public string RoleTemplateName { get; set; }

		/// <summary>
		/// RoleTemplateDescription
		/// </summary>
		public string RoleTemplateDescription { get; set; }

		/// <summary>
		/// RoleTemplateProductId
		/// </summary>
		public int RoleTemplateProductId { get; set; }

		/// <summary>
		/// ProductId
		/// </summary>
		public int ProductId { get; set; }

		/// <summary>
		/// RoleTemplateProductRoleMappingId
		/// </summary>
		public int RoleTemplateProductRoleMappingId { get; set; }

		/// <summary>
		/// ProductRoleId
		/// </summary>
		public string ProductRoleId { get; set; }

		/// <summary>
		/// ProductRoleName
		/// </summary>
		public string ProductRoleName { get; set; }

		/// <summary>
		/// RoleTemplateAdditionalProductRoleMappingId
		/// </summary>
		public int RoleTemplateAdditionalProductRoleMappingId { get; set; }

		/// <summary>
		/// AttributeName
		/// </summary>
		public string AttributeName { get; set; }

		/// <summary>
		/// AttributeValue
		/// </summary>
		public string AttributeValue { get; set; }

		/// <summary>
		/// Product Name
		/// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// RoleTemplate Product Batch
        /// </summary>
        public string RoleTemplateProductBatch { get; set; }
    }

    /// <summary>
    /// Product API (List of Properties and Roles) input JSON
    /// </summary>
    public class RoleTemplateProductBatch
    {
        #region Resident Portal
        /// <summary>
        /// List of Groups (Messaging groups)
        /// </summary>
        [JsonProperty(PropertyName = "MessageGroups", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> MessageGroups { get; set; }

        /// <summary>
        /// Staff user notification settings (optional)
        /// </summary>
        [JsonProperty(PropertyName = "Notifications", NullValueHandling = NullValueHandling.Ignore)]
        public Notifications Notifications { get; set; }

        public List<MessagingGroups> MessageGroupsList { get; set; }

        #endregion
    }



}

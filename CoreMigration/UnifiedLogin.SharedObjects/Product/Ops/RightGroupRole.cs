using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product.Ops
{
    public class RightGroupRole
    {
        /// <summary>
        /// Used to store the role id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Used to store the name of the role
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("is_marketplace_admin")]
        public string IsMarketPlaceAdmin { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("order_workflow_timeout")]
        public string OrderWorkflowTimeout { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("invoice_workflow_timeout")]
        public string InvoiceWorkflowTimeout { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("supplier_workflow_timeout")]
        public string SupplierWorkflowTimeout { get; set; }


        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("order_endorse_email_reminder_flag")]
        public string OrderEndorseEmailReminderFlag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("invoice_endorse_email_reminder_flag")]
        public string InvoiceEndorseEmailReminderFlag { get; set; }

        [JsonProperty("rights")]
        public RightGroup rights { get; set; }

        //    /// <summary>
        //    /// The id of the asset group
        //    /// </summary>
        //    [JsonProperty("group_list")]
        //    public IList<Group> GroupList { get; set; }

        //    /// <summary>
        //    /// The name of the asset group
        //    /// </summary>
        //    [JsonProperty("responsibility_list")]
        //    public IList<Right> ResponsibilityList { get; set; }
    }
}

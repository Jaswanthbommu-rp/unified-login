using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product.Ops
{
    /// <summary>
    /// An Ops role
    /// </summary>
    public class Role
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
        /// Used to store the desc of the role
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        

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
    }
}

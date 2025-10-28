using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Collections;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Products
	/// </summary>
    public class ProductUI : IProductUI, ICloneable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone() { return this.MemberwiseClone(); }


        /// <summary>
        /// Product Unique Id
        /// </summary>
        [JsonProperty(PropertyName = "productId")]
		public int ProductId { get; set; }

        /// <summary>
        /// A unique guid for the title for the product
        /// </summary>
        public Guid TitleUniqueId { get; set; }

        /// <summary>
        /// A unique id for the title for the product
        /// </summary>
        [JsonProperty(PropertyName = "titleID")]
        public string TitleId { get; set; }

        /// <summary>
        /// The css class for the product
        /// </summary>
        [JsonProperty(PropertyName = "className")]
        public string ClassName { get; set; }

        /// <summary>
        /// ClientId
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The url for the settings page for the product
        /// </summary>
        [JsonProperty(PropertyName = "settingsUrl")]
        public string SettingsUrl { get; set; }

        /// <summary>
        /// The url to launch the product
        /// </summary>
        [JsonProperty(PropertyName = "productUrl")]
        public string ProductUrl { get; set; }

		/// <summary>
		/// Activities List
		/// </summary>
        [JsonProperty(PropertyName = "activitiesList")]
        public IList<Activities> ActivitiesList { get; set; }

		/// <summary>
		/// IsNewTab
		/// </summary>
		[JsonProperty(PropertyName = "isNewTab")]
        public bool IsNewTab { get; set; } = false;

		/// <summary>
		/// The name of the product
		/// </summary>
		[JsonProperty(PropertyName = "ProductName")]
		public string ProductName { get; set; } = string.Empty;

		/// <summary>
		/// Product Type Unique Id
		/// </summary>
		[JsonProperty(PropertyName = "ProductTypeId")]
		public int ProductTypeId { get; set; }

		/// <summary>
		/// A description for the client
		/// </summary>
		[JsonProperty(PropertyName = "ProductDescription")]
		public string ProductDescription { get; set; } = "";

        /// <summary>
		/// Allow product to be set as favorite. Shows/hides the fave button in the UI.
		/// </summary>
        [JsonProperty(PropertyName = "IsAllowFavorite")]
		public bool IsAllowFavorite { get; set; } = false;

        /// <summary>
        /// Product is set as favorite
        /// </summary>
        [JsonProperty(PropertyName = "IsFavorite")]
		public bool IsFavorite { get; set; } = false;

		/// <summary>
		/// User has access to the product
		/// </summary>
		[JsonProperty(PropertyName = "HasAccess")]
		public bool HasAccess { get; set; } = false;

		/// <summary>
		/// Family Id where the product belong
		/// </summary>
		[JsonProperty(PropertyName = "FamilyId")]
		public int? FamilyId { get; set; } = 0;

		/// <summary>
		/// Family where the product belong
		/// </summary>
		[JsonProperty(PropertyName = "Family")]
		public string Family { get; set; } = string.Empty;

		/// <summary>
		/// Solution Id where the product belong
		/// </summary>
		[JsonProperty(PropertyName = "SolutionId")]
		public int SolutionId { get; set; } = 0;

		/// <summary>
		/// Solution where the product belong
		/// </summary>
		[JsonProperty(PropertyName = "Solution")]
		public string Solution { get; set; } = string.Empty;

		/// <summary>
		/// Sub solution of the product
		/// </summary>
		[JsonProperty(PropertyName = "Subsolution")]
		public string Subsolution { get; set; } = string.Empty;

		/// <summary>
		/// Product will be displayed under resource section in dashboard
		/// </summary>
		[JsonProperty(PropertyName = "IsResource")]
		public bool IsResource { get; set; }

		/// <summary>
		///  Learn more url
		/// </summary>
		[JsonProperty(PropertyName = "LearnMore")]
		public string LearnMore { get; set; } = "";

		/// <summary>
		///  Learn more url
		/// </summary>
		[JsonProperty(PropertyName = "ProductStatus")]
		public int ProductStatus { get; set; } = Convert.ToInt32(ProductBatchStatusType.Success);		

		/// <summary>
		/// Product Shown In AppSwitcher
		/// </summary>
		[JsonProperty(PropertyName = "ShowInAppSwitcher")]
		public bool ShowInAppSwitcher { get; set; } = false;

		/// <summary>
		/// Product Shown In UserList Filter
		/// </summary>
		[JsonProperty(PropertyName = "ShowInUserListFilter")]
		public bool ShowInUserListFilter { get; set; } = false;

		/// <summary>
		/// Product Code
		/// </summary>
		[JsonProperty(PropertyName = "ProductCode")]
		public string ProductCode { get; set; } = string.Empty;

        /// <summary>
        /// UDM Source Code
        /// </summary>
        [JsonProperty(PropertyName = "UDMSourceCode")]
		public string UDMSourceCode;

		/// <summary>
		/// Product Unique Id
		/// </summary>
		[JsonProperty(PropertyName = "productInstance")]
		public string ProductInstance { get; set; } = null;

		/// <summary>
		/// GreenBook Cares
		/// </summary>
		[JsonProperty(PropertyName = "greenBookCares")]
		public bool GreenBookCares { get; set; }

		/// <summary>
		/// [JsonProperty(PropertyName = "usePrimaryProperties")]
		/// </summary>
		public bool? UsePrimaryProperties { get; set; }

		[JsonProperty(PropertyName = "isInUDM")]
		public bool IsInUDM { get; set; }

		[JsonProperty(PropertyName = "active")]
		public bool Active { get; set; }
	}
}
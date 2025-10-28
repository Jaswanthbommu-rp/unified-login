using System;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for ProductUI
	/// </summary>
    public interface IProductUI
    {
		/// <summary>
		/// Activities List
		/// </summary>
		IList<Activities> ActivitiesList { get; set; }

		/// <summary>
		/// ClientId
		/// </summary>
		string ClientId { get; set; }

		/// <summary>
		/// The css class for the product
		/// </summary>
		string ClassName { get; set; }

		/// <summary>
		/// IsNewTab
		/// </summary>
		bool IsNewTab { get; set; }

		/// <summary>
		/// The url to launch the product
		/// </summary>
		string ProductUrl { get; set; }

		/// <summary>
		/// The url for the settings page for the product
		/// </summary>
		string SettingsUrl { get; set; }

		/// <summary>
		/// A unique id for the title for the product
		/// </summary>
		string TitleId { get; set; }

		/// <summary>
		/// A unique guid for the title for the product
		/// </summary>
		Guid TitleUniqueId { get; set; }

		/// <summary>
		/// Product Unique Id
		/// </summary>
		int ProductId { get; set; }

		/// <summary>
		/// Family Id where the product belong
		/// </summary>
		int? FamilyId { get; set; }

		/// <summary>
		/// Family where the product belong
		/// </summary>
		string Family { get; set; }

		/// <summary>
		/// Solution Id where the product belong
		/// </summary>
		int SolutionId { get; set; }

		/// <summary>
		/// Solution where the product belong
		/// </summary>
		string Solution { get; set; }

		/// <summary>
		/// The name of the product
		/// </summary>
		string ProductName { get; set; }

		/// <summary>
		/// Product Type Unique Id
		/// </summary>
		int ProductTypeId { get; set; }

		/// <summary>
		/// A description for the client
		/// </summary>
		string ProductDescription { get; set; }

        /// <summary>
		/// Allow product to be set as favorite. Shows/hides the fave button in the UI.
		/// </summary>
		bool IsAllowFavorite { get; set; }

        /// <summary>
        /// Product is set as favorite
        /// </summary>
        bool IsFavorite { get; set; }

		/// <summary>
		/// User has access to the product
		/// </summary>
		bool HasAccess { get; set; }

		/// <summary>
		/// Sub solution of the product
		/// </summary>
		string Subsolution { get; set; }

		/// <summary>
		/// Product will be displayed under resource section in dashboard
		/// </summary>
		bool IsResource { get; set; }

		/// <summary>
		///  Learn more url
		/// </summary>
		int ProductStatus { get; set; }		

		/// <summary>
		/// Product Shown In AppSwitcher
		/// </summary>		
		bool ShowInAppSwitcher { get; set; }
		/// <summary>
		/// Product Shown In UserList Filter
		/// </summary>		
		bool ShowInUserListFilter { get; set; } 
	}
}
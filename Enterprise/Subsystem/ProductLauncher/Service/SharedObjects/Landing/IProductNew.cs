using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProductNew
    {
        /// <summary>
        /// 
        /// </summary>
        Guid ProductGUID { get; set; }

        /// <summary>
        /// ProductId
        /// </summary>
        int ProductTypeId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int PartyId { get; set; }

        /// <summary>
        /// ProductId
        /// </summary>
        int FamilyId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string Family { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int SolutionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string Solution { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string Description { get; set; }
        
        /// <summary>
		/// 
		/// </summary>
		ProductSetting ClassName { get; set; }

        /// <summary>
		/// 
		/// </summary>
		ProductSetting NewTab { get; set; }

        /// <summary>
		/// 
		/// </summary>
		ProductSetting ProductUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        ProductSetting SettingsUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        ProductSetting TitleId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        ProductSetting TitleUniqueId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        IList<Activities> ActivitiesList { get; set; }        

        /// <summary>
        /// 
        /// </summary>
        bool IsFavorite { get; set; }

        /// <summary>
        /// 
        /// </summary>
        bool HasAccess { get; set; }
    }
}

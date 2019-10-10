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
    public class ProductNew : IProductNew
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid ProductGUID { get; set; } = Guid.Empty;

        /// <summary>
        /// ProductId
        /// </summary>
        public int ProductTypeId { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        public int PartyId { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
		/// 
		/// </summary>
		public ProductSetting ClassName { get; set; } = new ProductSetting();

        /// <summary>
		/// 
		/// </summary>
		public ProductSetting NewTab { get; set; } = new ProductSetting();

        /// <summary>
		/// 
		/// </summary>
		public ProductSetting ProductUrl { get; set; } = new ProductSetting();

        /// <summary>
        /// 
        /// </summary>
        public ProductSetting SettingsUrl { get; set; } = new ProductSetting();

        /// <summary>
        /// 
        /// </summary>
        public ProductSetting TitleId { get; set; } = new ProductSetting();

        /// <summary>
        /// 
        /// </summary>
        public ProductSetting TitleUniqueId { get; set; } = new ProductSetting();

        /// <summary>
        /// 
        /// </summary>
        public IList<Activities> ActivitiesList { get; set; } = new List<Activities>();

        
        /// <summary>
        /// 
        /// </summary>
        public bool IsFavorite { get; set; } = false;


        /// <summary>
        /// 
        /// </summary>
        public bool HasAccess { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public int FamilyId { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        public string Family { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public int SolutionId { get; set; } = 0;
        
        /// <summary>
        /// 
        /// </summary>
        public string Solution { get; set; } = string.Empty;
        
    }
}

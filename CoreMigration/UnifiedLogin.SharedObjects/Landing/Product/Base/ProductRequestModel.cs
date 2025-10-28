using UnifiedLogin.SharedObjects.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing.Product.Base
{
    /// <summary>
    /// Product Request Model
    /// </summary>
    public abstract class ProductRequestModel : IProductRequestModel
    {
        #region Ctor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        public ProductRequestModel(ProductEnum productId)
        {
            ProductId = productId;
        }
        #endregion

        /// <summary>
        /// Claim as Editor
        /// </summary>
        public DefaultUserClaim EditorClaim { get; set; }

        /// <summary>
        /// The persona id for the claiming editor user
        /// </summary>
        public long EditorPersonaId { get; set; }

        /// <summary>
        /// Product Id
        /// </summary>
        public ProductEnum ProductId { get; private set; }
    }
}

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
    public interface IProductRequestModel
    {
        /// <summary>
        /// Claim as Editor
        /// </summary>
        DefaultUserClaim EditorClaim { get; set; }

        /// <summary>
        /// The persona id for the claiming editor user
        /// </summary>
        long EditorPersonaId { get; set; }

        /// <summary>
        /// Product Id
        /// </summary>
        ProductEnum ProductId { get; }
    }
}

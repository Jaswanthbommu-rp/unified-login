using System;
using System.Collections.Generic;
using System.Text;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.Logic.EnterpriseRolesProperties
{
    /// <summary>
    /// Strategy interface for creating product-specific batches
    /// </summary>
    public interface IProductBatchStrategy
    {
        /// <summary>
        /// Product ID this strategy handles
        /// </summary>
        int ProductId { get; }

        /// <summary>
        /// Creates a product batch record
        /// </summary>
        Task<ProductBatch> CreateBatchAsync(ProductBatchContext context, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Context for creating product batches
    /// </summary>
    public class ProductBatchContext
    {
        public long EditorPersonaId { get; set; }
        public long SubjectPersonaId { get; set; }
        public int ProductId { get; set; }
        public Persona EditorPersona { get; set; }
        public Persona UserPersona { get; set; }
        public ListResponse PropertiesResponse { get; set; }
        public ListResponse RolesResponse { get; set; }
        public IList<ProductRole> ProductRoles { get; set; }
        public bool UsePrimaryProperties { get; set; }
        public bool IsExternalUser { get; set; }
        public ProductIntegrationTypeEnum IntegrationType { get; set; }
    }
}

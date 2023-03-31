using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    /// <summary>
    /// Manage Relationship Type repository calls
    /// </summary>
    public class ManageRelationshipType : IManageRelationshipType
    {
        #region Private Variables
        IRelationshipTypeRepository _relationshipTypeRepository;
        #endregion

        #region Constructors
        /// <summary>
        /// Relationship Type Repository Constructor
        /// </summary>
        /// <param name="relationshipTypeRepository">Relationship Type Repository</param>
        public ManageRelationshipType(IRelationshipTypeRepository relationshipTypeRepository)
        {
            _relationshipTypeRepository = relationshipTypeRepository;
        }

        /// <summary>
        /// Create a basic instance of the ManageOrganization Controller class
        /// </summary>
        public ManageRelationshipType()
        {
            _relationshipTypeRepository = new RelationshipTypeRepository();
        }
        #endregion

        /// <summary>
        /// Get RelationshipType
        /// </summary>
        /// <param name="relationshipTypeName">Relationship Type Name</param>
        /// <returns>List of RelationshipType object</returns>
        public IList<RelationshipType> GetRelationshipType(string relationshipTypeName)
        {
            return _relationshipTypeRepository.GetRelationshipType(relationshipTypeName);
        }
    }
}
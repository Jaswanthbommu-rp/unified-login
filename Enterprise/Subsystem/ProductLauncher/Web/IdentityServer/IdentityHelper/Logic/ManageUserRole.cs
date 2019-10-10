using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository;
using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Logic
{
    public class ManageUserRole
    {
        private IUserRoleRepository _userRoleRepository;
        private IOrganizationRepository _organizationRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userRoleRepository"></param>
        /// <param name="organizationRepository"></param>
        public ManageUserRole(IUserRoleRepository userRoleRepository, IOrganizationRepository organizationRepository)
        {
            _userRoleRepository = userRoleRepository;
            _organizationRepository = organizationRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        public ManageUserRole()
        {
            _userRoleRepository = new UserRoleRepository();
            _organizationRepository = new OrganizationRepository();
        }

        /// <summary>
        /// Get list of Roles by User Persona ID and product
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public IList<ProductRole> GetProductRolesByPersona(long personaId, ProductEnum productId)
        {
            if (personaId == 0)
            {
                throw new Exception("Invalid parameter personaId.");
            }

            return _userRoleRepository.GetProductRolesByPersona(personaId, productId);
        }

        /// <summary>
        /// Get list of Rights id by Party, product id and role id
        /// </summary>
        /// <param name="organizationPartyId">Company party id</param>
        /// <param name="organizationGuid">Company guid id</param>
        /// <param name="productId">Product Id</param>
        /// <param name="roleId">Role Id</param>
        /// <returns>The list of rights for the given role</returns>
        public IList<ProductRight> ListRightsByRole(long organizationPartyId, Guid organizationGuid, ProductEnum productId, long roleId)
        {
            if (organizationPartyId == 0)
            {
                throw new Exception("Invalid parameter partyId.");
            }
            if (roleId == 0)
            {
                throw new Exception("Invalid parameter roleId.");
            }

            // get the products for the given company, for now from our db but may be replaced with api call to bluebook
            IList<ProductUI> productListUI = _organizationRepository.GetProductsByCompany(organizationGuid);
            IList<int> productList = new List<int>();
            foreach (ProductUI prod in productListUI)
            {
                productList.Add(prod.ProductId);
            }

            return _userRoleRepository.ListRightsByRole(organizationPartyId, productList, productId, roleId);
        }
    }
}
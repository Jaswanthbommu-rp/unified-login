using Dapper;
using RP.Enterprise.Foundation.DataAccess.Component.Helper;
using RP.Enterprise.Foundation.DataAccess.Component.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.EmployeeAccess;
using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
	/// <summary>
	/// UnifiedLogin Repository
	/// </summary>
	public class UnifiedLoginRepository : BaseRepository, IUnifiedLoginRepository
	{
        IProductInternalSettingRepository _productInternalSettingRepository;
        #region Constructor
        /// <summary>
        /// UserManagement base Constructor
        /// </summary>
        public UnifiedLoginRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
            _productInternalSettingRepository = new ProductInternalSettingRepository();
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        public UnifiedLoginRepository(IRepository repository) : base(repository)
        {
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
        }
        #endregion

        #region public Roles methods
        /// <summary>
        /// Update Rights to Role
        /// </summary>
        public int LinkRightsToRole(IEnumerable<RightRoleAddRem> rightsList, int userId)
        {
            DynamicParameters param = new DynamicParameters();

            var procName = StoredProcNameConstants.SP_LinkRightsToRoles;

            dynamic p = new
            {
                CreatedBy = userId,
                NewRightID = 0
            };
            param.AddDynamicParams(p);

            using (var repository = GetRepository())
            {
                var tvp = new TableValueParmInfo();
                IEnumerable<string> col = new string[] { "RoleId", "RightValueTypeID", "IsDeleted" };
                tvp.OrderedColumnName = col;
                tvp.TableVariableName = "ManageRight";
                tvp.TableParamTypeName = "dbo.TYPROLE";
                tvp.StoredProcedureName = procName;

                var result = repository.ExecuteStoredProcWithTvp<RightRoleAddRem>(tvp, rightsList, param);

                return result;
            }
        }

        /// <summary>
        /// List of Companies
        /// </summary>       
        /// <returns>List of Users </returns>
        public List<UserDetail> ListUsers(string filter, string OrganizationTypeIds = null)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    Name = filter,
                    OrganizationTypeIds = OrganizationTypeIds
                };

                List<UserDetail> userList = new List<UserDetail>();
                var result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListUsers, param);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        userList.Add(new UserDetail { CompanyId = long.Parse(item.PartyId.ToString()), CompanyName = item.CompanyName, CompanyStatus = item.CompanyStatus, EmailId = item.NotificationEmail, FirstName = item.FirstName, LastName = item.LastName, UserId = int.Parse(item.UserId.ToString()), UserName = item.Username, UserType = item.UserType, Name3rdPartyIDP = item.ThirdPartyIDPDesc, PersonaId = item.PersonaId , PersonaRealPageId = item.PersonaRealPageId, UserStatus = item.UserStatus });
                    }
                    userList = userList.OrderBy(p => p.CompanyName).ToList();
                }
                return userList;
            }
        }

        /// <summary>
        /// List of Roles by Party ID
        /// </summary>
        /// <param name="partyId">Party ID</param>        
        /// <returns>List of Roles</returns>
        public List<ProductRole> ListRolesByParty(long partyId)
        {
            using (var repository = GetRepository())
            {
                List<ProductRole> rolesList = new List<ProductRole>();
                var procName = StoredProcNameConstants.SP_ListRolesByParty;

                IList<dynamic> result = repository.GetMany<dynamic>(procName, new { partyId }).ToList();
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        rolesList.Add(new ProductRole { ID = item.RoleID.ToString(), Name = item.Value, IsAssigned = false, Alias = item.RoleNickName });
                    }
                }
                return rolesList;
            }
        }



        /// <summary>
        /// List of Roles with Rights count
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="ulProductId">Product ID for Unified Login</param>  
        /// <param name="productIdList">Product ID's by Org</param>  
        /// <returns>List of Roles and rights count by PartyId and Product</returns>
        public List<RightRoleDetail> ListRoleWithRights(long partyId, long ulProductId, List<int> productIdList)
        {
            using (var repository = GetRepository())
            {
                var procName = StoredProcNameConstants.SP_ListRolesAssociatedWithRights;

                dynamic param = new
                {
                    PartyId = partyId,
                    ProductId = ulProductId,
                    TargetProductId = TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype") 
                };

                List<RightRoleDetail> rolesList = new List<RightRoleDetail>();
                var result = repository.GetMany<dynamic>(procName, param);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        rolesList.Add(new RightRoleDetail { RoleId = item.RoleId, RoleName = item.Role, IsAssigned = false, RoleType = item.RoleType, RightName = item.Right, RightId = item.RightId, RightValueTypeId = item.RightValueTypeId, IsDefaultRole = (bool)item.DefaultRole });
                    }
                }               


                return rolesList;
            }
        }


        /// <summary>
        /// List of Rights by Party Product
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <returns>List of  rights by PartyId and Product</returns>
        public List<ProductRight> ListRightForProductsByPartyId(long partyId, long productId)
        {
            using (var repository = GetRepository())
            {
                var procName = StoredProcNameConstants.SP_ListRightForProductsByPartyId;

                dynamic param = new
                {
                    PartyId = partyId,
                    ProductId = productId
                };

                List<ProductRight> rightsList = new List<ProductRight>();
                var result = repository.GetMany<dynamic>(procName, param);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        rightsList.Add(new ProductRight { ID = item.RightId, Description = item.value, Assigned = false, Alias = item.RightNickName }); 
                    }
                }
                return rightsList;
            }
        }

        /// <summary>
        /// List ALL Rights by Party Product
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="productId">Product ID</param>
        /// <param name="productIdList"></param>   
        /// <returns>List ALL  rights by PartyId and Product</returns>
        public List<ProductRight> ListAllRightsForProductsByPartyId(long partyId, long productId, List<int> productIdList)
        {
            using (var repository = GetRepository())
            {
                var procName = StoredProcNameConstants.SP_ListAllRights;

                dynamic param = new
                {
                    PartyId = partyId,
                    ProductId = productId,
                    TargetProductId = TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype")
                };

                List<ProductRight> rightsList = new List<ProductRight>();
                var result = repository.GetMany<dynamic>(procName, param);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        rightsList.Add(new ProductRight { ID = item.RightValueTypeId, Description = item.Right, Assigned = false, Alias = item.RightNickName }); //RightValueTypeId instead of RightId
                    }
                }
                return rightsList;
            }
        }

        /// <summary>
        /// List of Roles by Party Product
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="productId">Product ID</param>
        /// <param name="productIdList"></param>   
        /// <returns>List of Roles by PartyId and Product</returns>
        public List<ProductRole> ListRolesForProductsByPartyId(long partyId, long productId, List<int> productIdList)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    PartyId = partyId,
                    ProductId = productId,
                    TargetProductId = TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype")
                };

                var rolesList = new List<ProductRole>();
                var result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListRolesForProductsByPartyId, param);
                if (result == null) return rolesList;

                foreach (var item in result)
                {
                    rolesList.Add(new ProductRole { ID = item.RoleId.ToString(), Name = item.value, IsAssigned = false, Roletype = item.RoleType, Alias = item.RoleNickName, DefaultRole = item.DefaultRole.ToString() });
                }
                return rolesList;
            }
        }

        /// <summary>
        /// List of Companies
        /// </summary>       
        /// <returns>List of Companies </returns>
        public List<UnifiedLoginCompany> ListCompanies(string filter = "",string OrganizationTypeIds = null)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {  
                    Filter = filter,
                    OrganizationTypeIds = OrganizationTypeIds
                };

                List<UnifiedLoginCompany> compList = new List<UnifiedLoginCompany>();
                var result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListOrganizations, param);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        compList.Add(new UnifiedLoginCompany { CompanyId = long.Parse(item.BooksMasterId.ToString()), BooksCustomerMasterId = long.Parse(item.BooksCustomerMasterId.ToString() == string.Empty ? "0" : item.BooksCustomerMasterId.ToString()), CompanyName = item.Name, IsActive = int.Parse(item.IsActive.ToString()) == 1? true:false, PartyId = item.PartyId, CompanyRealPageId = item.OrganizationRealPageId.ToString(),  UserRealPageId = item.PersonRealPageId.ToString(), UserLoginAs = item.LoginName, Domain = item.Domain });
                    }
					compList = compList.OrderBy(p => p.CompanyName).ToList();
                }
                return compList;
            }
        }

        /// <summary>
        /// List of User ADGroups
        /// </summary>       
        /// <returns>List of User ADGroups </returns>
        public List<PersonaADGroup> GetPersonaADGroups(long personaId)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    PersonaId = personaId
                };

                var result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_GetADGroupsByPersonaId, param);
                var list = new List<PersonaADGroup>();
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        list.Add(new PersonaADGroup
                        {
                            ADGroupId = item.ADGroupId,
                            ADGroupName = item.ADGroupName,
                            ProductsCount = item.ProductsCount,
                            RightsCount = item.RightsCount
                        });
                    }
                }
                return list;
            }
        }

        //// <summary>
        /// List of OrgTypes ADGroups
        /// </summary>       
        /// <returns>List of OrgTypes ADGroups </returns>
        public List<OrgTypesADGroups> GetOrgTypesADGroups()
        {
            using (var repository = GetRepository())
            {
                var result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_GetOrganizationTypeADGroups, null);
                var list = new List<OrgTypesADGroups>();
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        list.Add(new OrgTypesADGroups
                        {
                            OrganizationTypeId = item.OrganizationTypeId,
                            OrganizationTypeName = item.OrganizationTypeName,
                            ADGroupId = item.ADGroupId,
                            ADGroupName = item.ADGroupName
                        });
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// List of Role and Right Det by Party Product
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <returns>List of Roles by PartyId and Product</returns>
        public List<RightRoleDetail> ListRoleRightDetForProductsByPartyId(long partyId, long productId, List<int> productIdList)
        {
            using (var repository = GetRepository())
            {
                var procName = StoredProcNameConstants.SP_ListRightsAssociatedWithRoles;

                dynamic param = new
                {
                    PartyId = partyId,
                    ProductId = productId,
                    TargetProductId = TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype")
                };

                List<RightRoleDetail> roleRightList = new List<RightRoleDetail>();
                var result = repository.GetMany<dynamic>(procName, param);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        roleRightList.Add(new RightRoleDetail { RoleId = item.RoleId, RoleName = item.Role, IsAssigned = false, RoleType = item.RoleType, RightId=item.RightId, RightName = item.Right, RightValueTypeId = item.RightValueTypeId });
                    }
                }
                return roleRightList;
            }
        }

        /// <summary>
        /// List of Roles with Rights count
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="productId">Product ID</param>
        /// <param name="productIdList"></param>   
        /// <returns>List of Roles and rights count by PartyId and Product</returns>
        public List<RightRoleDetail> ListRightWithRoles(long partyId, long productId, IList<int> productIdList)
        {
            using (var repository = GetRepository())
            {
                var procName = StoredProcNameConstants.SP_ListRightsAssociatedWithRoles;

                dynamic param = new
                {
                    PartyId = partyId,
                    ProductId = productId,
                    TargetProductId = TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype")
                };

                List<RightRoleDetail> rightsList = new List<RightRoleDetail>();
                var result = repository.GetMany<dynamic>(procName, param);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        rightsList.Add(new RightRoleDetail { RoleId = item.RoleId, RoleName = item.Role, IsAssigned = false, RoleType = item.RoleType, RightName = item.Right, RightId = item.RightId, RightValueTypeId = item.RightValueTypeId, RightNickName = item.RightNickName, RightDescription = item.RightDescription }); //RightsAssigned = item.count
                    }
                }
                return rightsList;
            }
        }

		/// <summary>
		/// List of Rights by Role
		/// </summary>
		/// <param name="partyId">Party ID</param>
		/// <param name="productIdList">Product ID</param>
		/// <param name="productId">Product ID</param>
		/// <param name="roleId">Role ID</param>   
		/// <returns>List of Rights by RoleId and Product</returns>
		public List<ProductRight> ListRightsByRole(long partyId, IList<int> productIdList, long productId, long roleId)
        {
            using (var repository = GetRepository())
            {
                var procName = StoredProcNameConstants.SP_ListRolesAssociatedWithRights;

                dynamic param = new
                {
                    PartyId = partyId,
                    ProductId = productId,
                    RoleId = roleId,
	                TargetProductId = TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype")
				};

                List<ProductRight> rightsList = new List<ProductRight>();
                var result = repository.GetMany<dynamic>(procName, param);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        rightsList.Add(new ProductRight { ID = item.RightValueTypeId, Description = item.Right, Alias = item.RightNickName, Assigned = true}); // RightValueTypeId instead of RightId
                    }
                }
                return rightsList;
            }
        }

        /// <summary>
        /// List of  Rights by Role
        /// </summary>
        /// <param name="partyId">Party ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <param name="rightId">Role ID</param>   
        /// <returns>List of Roles by RightId and Product</returns>
        public List<ProductRole> ListRolesByRight(long partyId, long productId, long rightId)
        {
            using (var repository = GetRepository())
            {
                var procName = StoredProcNameConstants.SP_ListRightsAssociatedWithRoles;

                dynamic param = new
                {
                    PartyId = partyId,
                    ProductId = productId,
                    RightId = rightId
                };

                List<ProductRole> rolesList = new List<ProductRole>();
                var result = repository.GetMany<dynamic>(procName, param);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        rolesList.Add(new ProductRole { ID = item.RoleId.ToString(), Name = item.Role, IsAssigned = true, Alias = item.RoleNickName });
                    }
                }
                return rolesList;
            }
        }

        /// <summary>
        /// List of Roles by User Persona ID
        /// </summary>
        /// <param name="userPersonaId">Persona ID</param>        
        /// <returns>List of Roles assigned to Persona</returns>
        public List<ProductRole> ListRolesAssignedToPersona(long userPersonaId)
        {
            using (var repository = GetRepository())
            {
                List<ProductRole> rolesList = new List<ProductRole>();
                var procName = StoredProcNameConstants.SP_ListRolesByParty;

                IList<dynamic> result = repository.GetMany<dynamic>(procName, new { userPersonaId }).ToList();
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        rolesList.Add(new ProductRole { ID = item.RoleID.ToString(), Name = item.Value, IsAssigned = false, Alias = item.RoleNickName });
                    }
                }
                return rolesList;
            }
        }

        /// <summary>
        /// List of Roles by User Persona ID
        /// </summary>
        /// <param name="userPersonaId">Persona ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <returns>List of Properties/Role assigned to Persona</returns>
        public List<PropertyRole> ListPropertyMappingByPersona(long userPersonaId, long productId)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    PersonaID = userPersonaId,
                    ProductID = productId
                };

                List<PropertyRole> propRoleList = new List<PropertyRole>();
                var result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListPropertyMapping, param);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        propRoleList.Add(new PropertyRole { RoleID = item.RoleID, PropID = item.PropertyID });
                    }
                }
                return propRoleList;
            }
        }

        /// <summary>
        /// List of Roles by User Persona ID
        /// </summary>
        /// <param name="userPersonaId">Persona ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <returns>List of Properties/Role assigned to Persona</returns>
        public List<Property> ListPropByPersona(long userPersonaId, long productId)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    PersonaID = userPersonaId,
                    ProductID = productId
                };

                List<Property> propList = new List<Property>();
                var result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListPropertyMapping, param);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        propList.Add(new Property { PropID = item.PropertyID });
                    }
                }
                return propList;
            }
        }

        /// <summary>
        /// List of Roles by User Persona ID
        /// </summary>       
        /// <returns>List of Category Types</returns>
        public List<CategoryType> GetCategoryType()
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {                   
                };

                List<CategoryType> roleList = new List<CategoryType>();
                var result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListSecurityStatus, null);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        roleList.Add(new CategoryType { CategoryName = item.CategoryType, Status = item.Status, StatusTypeid = item.StatusTypeId });
                    }
                }
                return roleList;
            }
        }

        /// <summary>
        /// Insert Property/Role to User
        /// </summary>
        /// <param name="userPersonaId">User Persona ID</param>      
        /// <param name="productId">Product ID</param>      
        /// <param name="property">Property</param>      
        /// <param name="role">User Role</param>   
        /// <param name="del">isDeleted</param>   
        /// <returns>List of Roles assigned to Persona</returns>
        public RepositoryResponse InsertDelAssignedPropRoleToUser(long userPersonaId, long productId, UserLocation property, UserAccessGroup role, long del = 0)
        {
            using (var repository = GetRepository())
            {
                RepositoryResponse repositoryResponse = new RepositoryResponse();
                dynamic param = new
                {
                    PersonaID = userPersonaId,
                    ProductID = productId,
                    RoleID = int.Parse(role.AccessGroupCode),
                    PropertyID = int.Parse(property.PropertyId),
                    Deleted = del
                };

                int i = repository.ExecuteNonQuery(StoredProcNameConstants.SP_CreatePropertyMapping, param);
                repositoryResponse.Id = i;

                return repositoryResponse;
            }
        }

        /// <summary>
        /// Insert Property/Role to User
        /// </summary>
        /// <param name="userPersonaId">User Persona ID</param>             
        /// <param name="role">User Role</param>   
        /// <param name="del">isDeleted</param>   
        /// <returns>List of Roles assigned to Persona</returns>
        public RepositoryResponse InsertAssignedRoleToUser(long userPersonaId, UserAccessGroup role, int userId, long del = 0)
        {
            using (var repository = GetRepository())
            {
                RepositoryResponse repositoryResponse = new RepositoryResponse();
                string schemaName = getRoleRightsSchemaName();
                var procName = schemaName?.Length > 0 ? $"{schemaName}.LinkPersonaToRole" : StoredProcNameConstants.SP_LinkPersonaToRole;

                dynamic param = new
                {
                    PersonaID = userPersonaId,
                    RoleID = int.Parse(role.AccessGroupCode),
                    CreatedBy = userId
                };

                int i = repository.ExecuteNonQuery(procName, param);
                repositoryResponse.Id = i;

                return repositoryResponse;
            }
        }

        /// <summary>
        /// Insert Property/Role to User
        /// </summary>
        /// <param name="userPersonaId">User Persona ID</param>      
        /// <param name="productId">Product ID</param>      
        /// <param name="propertyId">Property ID</param>      
        /// <param name="roleId">User Role ID</param>   
        /// <param name="del">isDeleted</param>   
        /// <returns>List of Roles assigned to Persona</returns>
        public RepositoryResponse InsertDelAssignedPropRoleToUserNew(long userPersonaId, long productId, long propertyId, long roleId, long del = 0)
        {
            using (var repository = GetRepository())
            {
                RepositoryResponse repositoryResponse = new RepositoryResponse();
                dynamic param = new
                {
                    PersonaID = userPersonaId,
                    ProductID = productId,
                    RoleID = roleId,
                    PropertyID = propertyId,
                    Deleted = del
                };

                int i = repository.ExecuteNonQuery(StoredProcNameConstants.SP_CreatePropertyMapping, param);
                repositoryResponse.Id = i;

                return repositoryResponse;
            }
        }

        /// <summary>
        /// Add new role
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="desc">User Persona ID</param>             
        /// <param name="roleTypeId">User Role</param>   
        /// <param name="roleCategoryId">isDeleted</param>   
        /// <param name="partyId">isDeleted</param>   
        /// <returns>Add new Role</returns>
        public RepositoryResponse AddCustomRole(string roleName, string  desc, long roleTypeId, long roleCategoryId , long partyId, int userId,string OrganizationType)
        {
            using (var repository = GetRepository())
            {
                RepositoryResponse repositoryResponse = new RepositoryResponse();
                var procName = StoredProcNameConstants.SP_CreateRole;

                dynamic param = new
                {
                    RoleName = roleName,
                    Description = desc,
                    RoleTypeID = roleTypeId,
                    RoleCategoryId = roleCategoryId,
                    PartyID = partyId,
                    CreatedBy = userId,
                    OrganizationType = OrganizationType,
                    RoleID = 0
                
                };

                var result = repository.GetOne<dynamic>(procName, param);

                long roleId = result.RoleID;
                repositoryResponse.Id = roleId;
                if(result.ErrorMessage.Trim() != string.Empty)
                {
                    repositoryResponse.ErrorMessage = result.ErrorMessage;
                }
                
                return repositoryResponse;
            }
        }

        /// <summary>
        /// Delete role
        /// </summary>
        /// <param name="roleId">User Role</param>                    
        /// <returns>Deletes Role Response</returns>
        public RepositoryResponse DeleteRole(long roleId)
        {
            using (var repository = GetRepository())
            {
                RepositoryResponse repositoryResponse = new RepositoryResponse();
                var procName = StoredProcNameConstants.SP_DeleteRole;

                dynamic param = new
                {                   
                    RoleId = roleId
                };

                var result = repository.GetOne<dynamic>(procName, param);

                repositoryResponse.Id = roleId;
                if (result != null && result.ErrorMessage.Trim() != string.Empty)
                {
                    repositoryResponse.ErrorMessage = result.ErrorMessage;
                }

                return repositoryResponse;
            }
        }

        /// <summary>
        /// Set Default role
        /// </summary>
        /// <param name="roleId">User Role</param>                    
        /// <returns>Deletes Role Response</returns>
        public RepositoryResponse SetDefaultRole(long roleId,long partyid, int userId)
        {
            using (var repository = GetRepository())
            {
                RepositoryResponse repositoryResponse = new RepositoryResponse();
                string schemaName = getRoleRightsSchemaName();
                var procName = StoredProcNameConstants.SP_SetDefaulteRole;
                dynamic param;
                if (schemaName == "Security")
                {
                    param = new
                    {
                        RoleId = roleId,
                        PartyId = partyid,
                        CreatedBy = userId
                    };
                }
                else {
                    param = new
                    {
                        RoleId = roleId,
                        CreatedBy = userId
                    };
                }
               

                var result = repository.GetOne<dynamic>(procName, param);

                repositoryResponse.Id = roleId;
                if (result != null && result.ErrorMessage.Trim() != string.Empty)
                {
                    repositoryResponse.ErrorMessage = result.ErrorMessage;
                }

                return repositoryResponse;
            }
        }

        /// <summary>
        /// Add new role
        /// </summary>
        /// <param name="roleId">User Role</param>   
        /// <param name="roleName"></param>
        /// <param name="desc">User Persona ID</param>
        /// <returns>Add new Role Response</returns>
        public RepositoryResponse UpdateCustomRole(long roleId ,string roleName, string desc, int userId)
        {
            using (var repository = GetRepository())
            {
                RepositoryResponse repositoryResponse = new RepositoryResponse();
                var procName = StoredProcNameConstants.SP_UpdateRole;

                dynamic param = new
                {
                    RoleName = roleName,
                    Description = desc,
                    RoleId = roleId,
                    CreatedBy = userId
                };

                var result = repository.GetOne<dynamic>(procName, param);
              
                repositoryResponse.Id = roleId;
                if (result.ErrorMessage.Trim() != string.Empty)
                {
                    repositoryResponse.ErrorMessage = result.ErrorMessage;
                }

                return repositoryResponse;
            }
        }
        #endregion
        #region Private Methods
        private string getRoleRightsSchemaName()
        {
            RPObjectCache rpcache = new RPObjectCache();

            var cacheKey = "getRoleRightsSchemaName_" + (int)ProductEnum.UnifiedPlatform;
            string schemaName = rpcache.GetFromCache<string>(cacheKey, 60, () =>
            {
                var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
                return productInternalSettingList.FirstOrDefault(s => s.Name.Equals("RolesRightsSchemaName", StringComparison.OrdinalIgnoreCase))?.Value;
            });

            return schemaName;
        }
        #endregion
    }
}
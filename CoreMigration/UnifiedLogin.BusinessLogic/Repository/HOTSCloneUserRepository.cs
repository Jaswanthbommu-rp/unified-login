using Dapper;
using Newtonsoft.Json;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Hots;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Data;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
{
    public class HOTSCloneUserRepository : BaseRepository, IHOTSCloneUserRepository
    {
        private DefaultUserClaim _userClaims;
        #region Constructor

        /// <summary>
        /// Base constructor
        /// </summary>
        public HOTSCloneUserRepository(DefaultUserClaim userClaims) : base(DbConnectionEnum.IdpConfigurationDb)
        {
            _userClaims = userClaims;
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userClaims"></param>
        public HOTSCloneUserRepository(IRepository repository, DefaultUserClaim userClaims) : base(repository)
        {
            _userClaims = userClaims;
        }

        #endregion

        public IList<BaseLineCustomerCompanyUser> ListUsers(long OrganizationId)
        {

            var procName = StoredProcNameConstants.SP_ListHotsBaseOrganizationUsers;

            dynamic param = new
            {
                OrganizationId = OrganizationId
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetMany<BaseLineCustomerCompanyUser>(procName, param);
                return result;
            }
        }

        public List<PersonaProductUserDetails> GetUserProducts(long personaId)
        {
            var procName = StoredProcNameConstants.SP_ListProductsByPersonaId;

            dynamic param = new
            {
                PersonaId = personaId,
                ProductStatusValue = ((Int32)ProductBatchStatusType.Success).ToString()
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetMany<PersonaProductUserDetails>(procName, param);
                return result;
            }

        }

        public Guid GetBaseCompanyUPFMId(Guid cloneUpfmId)
        {
            var procName = StoredProcNameConstants.SP_GetBaseCompanyUPFMId;
            dynamic param = new
            {
                RealPageId = cloneUpfmId
            };

            using (var repository = GetRepository())
            {
                return repository.GetOne<Guid>(procName, param);
            }
        }

        public UserLoginOnly GetUserLoginOnly(string enterpriseUserName)
        {
            UserLoginOnly userLogin = new UserLoginOnly();
            dynamic param = new { EnterpriseUserName = enterpriseUserName };

            using (var repository = GetRepository())
            {
                userLogin = repository.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, param);
            }

            return userLogin;
        }

        public IList<Persona> ListPersona(Guid realPageId)
        {
            dynamic param = new
            {
                RealPageId = realPageId
            };

            using (var repository = GetRepository())
            {
                IList<Persona> personaList = repository.GetMany<Persona>(StoredProcNameConstants.SP_ListPersona, param);
                return personaList;
            }
        }

        public HotsUser CreateUser(DefaultUserClaim cloneCompanyAdminUserClaim, long partyId, BaseLineCustomerCompanyUser user, IProfileDetail baseUserProfile, List<ProductBatch> productBatch, UserLogin userLogin)
        {
            HotsUser hotsUser = new HotsUser();
            string loginName;
            loginName = getLoginName(partyId, baseUserProfile);

            dynamic param = new
            {
                FirstName = baseUserProfile.FirstName,
                MiddleName = baseUserProfile.MiddleName,
                LastName = baseUserProfile.LastName,
                UserTypeId = baseUserProfile.UserTypeId,
                ThirdPartyIDP = true,
                LoginName = loginName,
                NotificationEmail = string.Empty,
                UserEffectiveDate = DateTime.UtcNow,
                UserExpirationDate = new DateTime(9999, 12, 31),
                Phone = baseUserProfile.TelecommunicationNumber?.Count > 0 ? baseUserProfile.TelecommunicationNumber[0].PhoneNumber : string.Empty,
                PhoneType = PhoneType.Work.ToString(), // default to work
                PreferredContactMethod = string.Empty,
                Title = baseUserProfile.Title,
                Suffix = baseUserProfile.Suffix,
                Pwdhash = string.Empty,
                PwdSalt = string.Empty,
                CreateUserSourceType = "HOTS",
                OrganizationId = partyId,
                EmployeeId = baseUserProfile.EmployeeId
            };



            using (var repository = GetRepository())
            {
                repository.UnitOfWork.BeginTransaction();
                try
                {
                    //Get Impersonator Information
                    UserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
                    if (_userClaims.ImpersonatedBy != Guid.Empty)
                    {
                        impersonatorUserLoginOnly = repository.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, new { RealPageId = _userClaims.ImpersonatedBy });
                    }

                    // user creation
                    var spResult = repository.GetOne<dynamic>(EnterpriseStoredProcNameConstants.SP_CreateUnityUser, param);

                    // get real page id
                    var newUserRealPageId = spResult.RealPageId.ToString();

                    // get persona id & user id
                    var newUserPersonaId = repository.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona, new { RealPageId = newUserRealPageId });
                    var userId = repository.GetOne<long>(StoredProcNameConstants.SP_GetUserLoginOnly, new { RealPageId = newUserRealPageId });

                    if (!string.IsNullOrEmpty(userLogin.Password) && !string.IsNullOrEmpty(userLogin.PasswordSalt) && !string.IsNullOrEmpty(userLogin.PasswordHash))
                    {
                        //
                        var updatePassword = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateHotsCloneUserPassword, new { UserId = userId, PasswordHash = userLogin.PasswordHash, PasswordSalt = userLogin.PasswordSalt });
                        if (updatePassword.Id == userId)
                        {
                            hotsUser.ClonePassword = userLogin.Password;
                        }
                    }

                    bool superUser = false;
                    superUser = baseUserProfile.UserTypeId == 402;
                    // Add products
                    if (productBatch?.Count > 0) //TODO: remove UL product from list?
                        SaveProductBatch(repository, cloneCompanyAdminUserClaim.PersonaId, newUserPersonaId, cloneCompanyAdminUserClaim.UserRealPageGuid, productBatch, superUser, impersonatorUserLoginOnly.UserId);

                    //COMMIT THE CHANGE
                    repository.UnitOfWork.Commit();

                    hotsUser.BaselineUserId = baseUserProfile.userLogin.UserId;
                    hotsUser.BaselineUserName = baseUserProfile.userLogin.LoginName;
                    hotsUser.CloneUserId = userId;
                    hotsUser.CloneUserName = loginName;
                    hotsUser.ClonePersonaId = newUserPersonaId;
                }
                catch (Exception)
                {
                    repository.UnitOfWork.Rollback();
                    throw;
                }
            }

            return hotsUser;
        }

        /// <summary>
        /// Used to link a cloned company to a baseline company when using HOTS
        /// </summary>
        /// <param name="baselineCompanyRealPageId"></param>
        /// <param name="cloneCompanyRealPageId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public RepositoryResponse InsertHotsCompanyRelationship(Guid baselineCompanyRealPageId, Guid cloneCompanyRealPageId, int userId)
        {
            dynamic param = new
            {
                BaseLineCompany = baselineCompanyRealPageId,
                CloneCompany = cloneCompanyRealPageId,
                UserId = userId
            };

            using (var repository = GetRepository())
            {
                return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertHotsCompanyRelationship, param);
            }
        }

        /// <summary>
        /// Used to link a cloned property to a baseline property when using HOTS
        /// </summary>
        /// <param name="baselinePropertyInstanceId"></param>
        /// <param name="clonePropertyInstanceId"></param>
        /// <param name="cloneCompanyRealPageId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public RepositoryResponse InsertHotsPropertyRelationship(Guid baselinePropertyInstanceId, Guid clonePropertyInstanceId, Guid cloneCompanyRealPageId, int userId)
        {
            dynamic param = new
            {
                BaseLineProperty = baselinePropertyInstanceId,
                CloneProperty = clonePropertyInstanceId,
                CloneCompany = cloneCompanyRealPageId,
                UserId = userId
            };

            using (var repository = GetRepository())
            {
                return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertHotsPropertyRelationship, param);
            }
        }

        private string getLoginName(long partyId, IProfileDetail baseUserProfile)
        {
            return string.Concat(baseUserProfile.FirstName.Substring(0), baseUserProfile.LastName, partyId.ToString(), "@realpage.com");
        }

        private void SaveProductBatch(IRepository repository, long editorUserPersonaId, long subjectUserPersonaId, Guid editorUserRealPageId,
            IList<ProductBatch> userProductList, bool superUser, long impersonatorUserId)
        {
            //var productRepo = new ProductRepository();
            var batchGroup = CreateBatchProcessGroup(repository);
            var inputJsonAdmin = new RolePropertyList() { PropertyRoleList = new List<PropertyRoleList>(), PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = true };

            foreach (var prod in userProductList)
            {
                string inputJsonText = JsonConvert.SerializeObject(superUser ? inputJsonAdmin : prod.InputJson);

                dynamic productBatch = new
                {
                    PersonRealPageId = editorUserRealPageId,
                    CreateUserPersonaId = editorUserPersonaId,
                    AssignUserPersonaId = subjectUserPersonaId,
                    ProductId = prod.ProductId,
                    BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                    StatusTypeId = 5,
                    RetryCount = 0,
                    InputJson = inputJsonText,
                    CorrelationId = Guid.NewGuid().ToString(),
                    ImpersonatorUserId = impersonatorUserId,
                    BatchProcessTypeId = BatchProcessType.CreateUpdateProductUser,
                    UseAPIV2 = true
                };

                var repositoryResponse = repository.Execute<dynamic>(StoredProcNameConstants.SP_CreateProductBatch, productBatch);

                //In-case of an error creating a product batch record, append the ProductCode to the ErrorReason
                if (repositoryResponse.Id == 0)
                {
                    throw new Exception($"Exception while inserting product with code {prod.ProductId} in the product batch.");
                }
            }
        }

        private BatchProcessorGroup CreateBatchProcessGroup(IRepository repo)
        {
            {
                DynamicParameters param = new DynamicParameters();
                int groupID = 0;
                param.Add("@BatchProcessorGroupID", groupID, dbType: DbType.Int32, direction: ParameterDirection.Output);

                try
                {
                    var a = repo.Execute(StoredProcNameConstants.SP_CreateBatchProcessorGroup, param);
                    groupID = param.Get<int>("@BatchProcessorGroupID");
                }
                catch (Exception ex)
                {
                }

                return new BatchProcessorGroup()
                {
                    BatchProcessorGroupId = groupID,
                    BatchProcessorGroupActivityLogged = false
                };
            }
        }
    }
}

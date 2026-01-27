using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Enum;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.Repository
{
    /// <summary>
    /// Repository for batch product bulk update operations
    /// </summary>
    public class BatchProductBulkUpdateRepositoryV2 : BaseRepository, IBatchProductBulkUpdateRepository
    {
        private readonly DefaultUserClaim _userClaim;

        #region Constructor

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="userClaim">User claim information</param>
        public BatchProductBulkUpdateRepositoryV2(DefaultUserClaim userClaim) : base(DbConnectionEnum.IdpConfigurationDb)
        {
            _userClaim = userClaim ?? throw new ArgumentNullException(nameof(userClaim));
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository">Repository instance</param>
        /// <param name="userClaim">User claim information</param>
        public BatchProductBulkUpdateRepositoryV2(IRepository repository, DefaultUserClaim userClaim) : base(repository)
        {
            _userClaim = userClaim ?? throw new ArgumentNullException(nameof(userClaim));
        }

        #endregion

        /// <inheritdoc />
        public bool SaveProductBatch(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            Guid editorUserRealPageId,
            IList<ProductBatch> userProductList,
            string onesiteWithOherProductsJson,
            bool isOnesiteMix,
            int batchProcessType,
            long impersonatorUserId,
            string inputAOJSON)
        {
            if (userProductList == null || !userProductList.Any())
            {
                return false;
            }

            var batchGroup = CreateBatchProcessGroup();

            using (var repository = GetRepository())
            {
                foreach (var prod in userProductList)
                {
                    int statusType = (prod.ProductId != (int)ProductEnum.AssetOptimizer && prod.InputJson.IsAssigned)
                        ? batchProcessType
                        : (int)BatchProcessType.CreateUpdateProductUser;

                    if (prod.ProductId == (int)ProductEnum.KnockCRM)
                    {
                        statusType = (int)BatchProcessType.CreateUpdateProductUser;
                    }

                    string inputJson = JsonConvert.SerializeObject(prod.InputJson);

                    if (prod.ProductId == (int)ProductEnum.OneSite && isOnesiteMix)
                    {
                        inputJson = onesiteWithOherProductsJson;
                    }

                    if (prod.ProductId == (int)ProductEnum.AssetOptimizer)
                    {
                        inputJson = inputAOJSON;
                    }

                    dynamic productBatch = new
                    {
                        PersonRealPageId = editorUserRealPageId,
                        CreateUserPersonaId = editorUserPersonaId,
                        AssignUserPersonaId = subjectUserPersonaId,
                        ProductId = prod.ProductId,
                        StatusTypeId = 5,
                        RetryCount = 0,
                        InputJson = inputJson,
                        CorrelationId = Guid.NewGuid().ToString(),
                        BatchProcessTypeId = statusType,
                        BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                        ImpersonatorUserId = impersonatorUserId
                    };

                    var repositoryResponse = repository.Execute<dynamic>(StoredProcNameConstants.SP_CreateProductBatch, productBatch);

                    // In-case of an error creating a product batch record, append the ProductCode to the ErrorReason
                    if (repositoryResponse != null && repositoryResponse.Id == 0)
                    {
                        throw new Exception($"Exception while inserting product with code {prod.ProductId} in the product batch.");
                    }
                }

                return true;
            }
        }

        /// <inheritdoc />
        public IList<SamlAttributes> CreateBatch(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            Guid editorUserRealPageId,
            int productId,
            int retryCheckCount,
            int statusCheckSleep,
            string defaultUserRole,
            long impersonatorUserId)
        {
            IList<SamlAttributes> samlAttributesDetails = new List<SamlAttributes>();
            SamlRepository samlRepository = new SamlRepository();

            using (var repository = GetRepository())
            {
                repository.UnitOfWork.BeginTransaction();

                List<string> roleList = new List<string> { defaultUserRole };
                var batchGroup = CreateBatchProcessGroup(repository);

                dynamic productBatch = new
                {
                    PersonRealPageId = editorUserRealPageId,
                    CreateUserPersonaId = editorUserPersonaId,
                    AssignUserPersonaId = subjectUserPersonaId,
                    ProductId = productId,
                    StatusTypeId = 5,
                    RetryCount = 0,
                    BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                    ImpersonatorUserId = impersonatorUserId,
                    InputJson = JsonConvert.SerializeObject(new RolePropertyList
                    {
                        PropertyList = new List<string> { "-1" },
                        RoleList = roleList,
                        IsAssigned = true
                    }),
                };

                var repositoryResponse = repository.Execute<dynamic>(StoredProcNameConstants.SP_CreateProductBatch, productBatch);

                if (repositoryResponse.Id == 0)
                {
                    throw new Exception($"Exception while inserting product {productId} in the product batch.");
                }

                repository.UnitOfWork.Commit();

                // Poll for batch completion
                while (retryCheckCount >= 0)
                {
                    System.Threading.Thread.Sleep(statusCheckSleep);
                    var listUserBatchProductDetails = GetUserBatchDetails(
                        batchGroup.BatchProcessorGroupId,
                        editorUserPersonaId,
                        subjectUserPersonaId);

                    if (listUserBatchProductDetails.Any(a => a.StatusTypeId == 8))
                    {
                        samlAttributesDetails = samlRepository.GetProductSamlDetails(subjectUserPersonaId, productId);
                        if (!samlAttributesDetails.Any())
                        {
                            retryCheckCount--;
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(statusCheckSleep);
                            return samlAttributesDetails;
                        }
                    }
                    else if (listUserBatchProductDetails.Any(a => a.StatusTypeId == 7))
                    {
                        return samlAttributesDetails;
                    }
                    else
                    {
                        retryCheckCount--;
                    }
                }
            }

            return samlAttributesDetails;
        }

        /// <inheritdoc />
        public IList<UserBatchProductDetail> GetUserBatchDetails(int batchGroupId, long editorUserPersonId, long subjectUserPersonId)
        {
            using (var repo = GetRepository())
            {
                var data = repo.GetMany<UserBatchProductDetail>(
                    StoredProcNameConstants.SP_GetUserBatchRecords,
                    new
                    {
                        batchProcessorGroupId = batchGroupId,
                        editorUserPersonId = editorUserPersonId,
                        subjectUserPersonId = subjectUserPersonId
                    }).ToList();

                return data;
            }
        }

        /// <inheritdoc />
        public bool UpdateEnterpriseRoleProductBatch(long productBatchId, int statusTypeId)
        {
            using (var repository = GetRepository())
            {
                var result = repository.Execute<bool>(
                    StoredProcNameConstants.SP_UpdateEnterpriseRoleProductBatch,
                    new { productBatchId, statusTypeId });

                return result;
            }
        }

        /// <inheritdoc />
        public bool UpdatePrimaryPropertyProductBatch(long productBatchId, int statusTypeId)
        {
            using (var repository = GetRepository())
            {
                var result = repository.Execute<bool>(
                    StoredProcNameConstants.SP_UpdatePrimaryPropertyProductBatch,
                    new { productBatchId, statusTypeId });

                return result;
            }
        }

        /// <inheritdoc />
        public bool UpdateBulkUserProductBatch(long productBatchId, int statusTypeId)
        {
            using (var repository = GetRepository())
            {
                var result = repository.Execute<bool>(
                    StoredProcNameConstants.SP_UpdateBulkUserProductBatch,
                    new { productBatchId, statusTypeId });

                return result;
            }
        }

        /// <inheritdoc />
        public void UpdateUnifiedPlatFormRole(int roleId, long editorUserId, long userPersonaId, bool deleteRole = false)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    personaID = userPersonaId,
                    roleID = roleId,
                    IsDeleted = deleteRole,
                    CreatedBy = editorUserId,
                    personaPrivilgeID = 0
                };

                repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonaToRole, param);
            }
        }

        #region Private Methods

        private BatchProcessorGroup CreateBatchProcessGroup()
        {
            using (var repo = GetRepository())
            {
                return CreateBatchProcessGroup(repo);
            }
        }

        private BatchProcessorGroup CreateBatchProcessGroup(IRepository repo)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@BatchProcessorGroupID", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            try
            {
                repo.Execute(StoredProcNameConstants.SP_CreateBatchProcessorGroup, param);
                var groupID = param.Get<int>("@BatchProcessorGroupID");

                return new BatchProcessorGroup
                {
                    BatchProcessorGroupId = groupID,
                    BatchProcessorGroupActivityLogged = false
                };
            }
            catch (Exception ex)
            {
                // Log the exception
                return new BatchProcessorGroup
                {
                    BatchProcessorGroupId = 0,
                    BatchProcessorGroupActivityLogged = false
                };
            }
        }

        #endregion
    }
}
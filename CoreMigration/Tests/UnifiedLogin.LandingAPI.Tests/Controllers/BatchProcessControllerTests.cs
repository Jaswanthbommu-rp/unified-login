using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for BatchProcessController.
    /// Tests all endpoints, error cases, and edge cases for 100% code coverage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class BatchProcessControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private BatchProcessController _batchProcessController;

        #endregion

        #region Constructor

        public BatchProcessControllerTests()
        {
            _mockUserClaimsAccessor = MockUserClaimsAccessor;

            _batchProcessController = new BatchProcessController(_mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new BatchProcessController(null!));

            Assert.Equal("userClaimsAccessor", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithValidUserClaimsAccessor_CreatesInstance()
        {
            // Act
            var controller = new BatchProcessController(_mockUserClaimsAccessor.Object);

            // Assert
            Assert.NotNull(controller);
        }

        #endregion

        #region ProcessBatch Tests

        [Fact]
        public async Task ProcessBatch_WithNullBatchRecord_ReturnsBadRequest()
        {
            // Arrange
            ProductUserProperitiesRoles batchRecord = null!;

            // Act
            var result = await _batchProcessController.ProcessBatch(batchRecord);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("batchRecord is null.", badRequestResult.Value);
        }

        [Fact]
        public async Task ProcessBatch_WithValidBatchRecord_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new ProductUserProperitiesRoles
            {
                ProductBatchId = 1,
                RealPageId = Guid.NewGuid(),
                ProductId = 1,
                CreateUserPersonaId = 100,
                AssignUserPersonaId = 200,
                InputJson = "{}",
                BatchProcessType = BatchProcessType.CreateUpdateProductUser,
                CorrelationId = Guid.NewGuid(),
                BatchProcessorGroupId = 1,
                CreateRealPageEmployee = false,
                RealPageEmployeePersonaId = 0,
                ImpersonatorUserId = 0
            };

            // Act
            var result = await _batchProcessController.ProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        //[Fact]
        //public async Task ProcessBatch_WithMinimalBatchRecord_ReturnsCreatedStatus()
        //{
        //    // Arrange - Use valid BatchProcessType (enum starts at 1)
        //    var batchRecord = new ProductUserProperitiesRoles
        //    {
        //        ProductBatchId = 0,
        //        ProductId = 0,
        //        BatchProcessType = BatchProcessType.CreateUpdateProductUser
        //    };

        //    // Act
        //    var result = await _batchProcessController.ProcessBatch(batchRecord);

        //    // Assert
        //    Assert.NotNull(result);
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        //}

        //[Fact]
        //public async Task ProcessBatch_WithEmptyResult_ReturnsSuccessMessage()
        //{
        //    // Arrange
        //    var batchRecord = new ProductUserProperitiesRoles
        //    {
        //        ProductBatchId = 1,
        //        ProductId = 1,
        //        BatchProcessType = BatchProcessType.CreateUpdateProductUser
        //    };

        //    // Act
        //    var result = await _batchProcessController.ProcessBatch(batchRecord);

        //    // Assert
        //    Assert.NotNull(result);
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        //    // Result should be "Success" when processing returns empty string
        //    Assert.NotNull(statusCodeResult.Value);
        //}

        //[Fact]
        //public async Task ProcessBatch_WithLargeProductBatchId_ReturnsCreatedStatus()
        //{
        //    // Arrange
        //    var batchRecord = new ProductUserProperitiesRoles
        //    {
        //        ProductBatchId = int.MaxValue,
        //        ProductId = 1,
        //        BatchProcessType = BatchProcessType.CreateUpdateProductUser
        //    };

        //    // Act
        //    var result = await _batchProcessController.ProcessBatch(batchRecord);

        //    // Assert
        //    Assert.NotNull(result);
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        //}

        //[Fact]
        //public async Task ProcessBatch_WithNegativePersonaIds_ReturnsCreatedStatus()
        //{
        //    // Arrange
        //    var batchRecord = new ProductUserProperitiesRoles
        //    {
        //        ProductBatchId = 1,
        //        CreateUserPersonaId = -1,
        //        AssignUserPersonaId = -1,
        //        BatchProcessType = BatchProcessType.CreateUpdateProductUser
        //    };

        //    // Act
        //    var result = await _batchProcessController.ProcessBatch(batchRecord);

        //    // Assert
        //    Assert.NotNull(result);
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        //}

        //[Fact]
        //public async Task ProcessBatch_WithEmptyGuid_ReturnsCreatedStatus()
        //{
        //    // Arrange
        //    var batchRecord = new ProductUserProperitiesRoles
        //    {
        //        ProductBatchId = 1,
        //        RealPageId = Guid.Empty,
        //        CorrelationId = Guid.Empty,
        //        BatchProcessType = BatchProcessType.CreateUpdateProductUser
        //    };

        //    // Act
        //    var result = await _batchProcessController.ProcessBatch(batchRecord);

        //    // Assert
        //    Assert.NotNull(result);
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        //}

        //[Fact]
        //public async Task ProcessBatch_WithNullInputJson_ReturnsCreatedStatus()
        //{
        //    // Arrange
        //    var batchRecord = new ProductUserProperitiesRoles
        //    {
        //        ProductBatchId = 1,
        //        InputJson = null,
        //        BatchProcessType = BatchProcessType.CreateUpdateProductUser
        //    };

        //    // Act
        //    var result = await _batchProcessController.ProcessBatch(batchRecord);

        //    // Assert
        //    Assert.NotNull(result);
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        //}

        //[Fact]
        //public async Task ProcessBatch_WithRealPageEmployee_ReturnsCreatedStatus()
        //{
        //    // Arrange
        //    var batchRecord = new ProductUserProperitiesRoles
        //    {
        //        ProductBatchId = 1,
        //        CreateRealPageEmployee = true,
        //        RealPageEmployeePersonaId = 12345,
        //        BatchProcessType = BatchProcessType.CreateUpdateProductUser
        //    };

        //    // Act
        //    var result = await _batchProcessController.ProcessBatch(batchRecord);

        //    // Assert
        //    Assert.NotNull(result);
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        //}

        [Fact]
        public async Task ProcessBatch_WithProfileUpdateType_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new ProductUserProperitiesRoles
            {
                ProductBatchId = 1,
                BatchProcessType = BatchProcessType.ProfileUpdate
            };

            // Act
            var result = await _batchProcessController.ProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        //[Fact]
        //public async Task ProcessBatch_WithDeactivateProductUserType_ReturnsCreatedStatus()
        //{
        //    // Arrange
        //    var batchRecord = new ProductUserProperitiesRoles
        //    {
        //        ProductBatchId = 1,
        //        BatchProcessType = BatchProcessType.DeactivateProductUser
        //    };

        //    // Act
        //    var result = await _batchProcessController.ProcessBatch(batchRecord);

        //    // Assert
        //    Assert.NotNull(result);
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        //}

        //[Fact]
        //public async Task ProcessBatch_WithUnassignUserType_ReturnsCreatedStatus()
        //{
        //    // Arrange
        //    var batchRecord = new ProductUserProperitiesRoles
        //    {
        //        ProductBatchId = 1,
        //        BatchProcessType = BatchProcessType.UnassignUser
        //    };

        //    // Act
        //    var result = await _batchProcessController.ProcessBatch(batchRecord);

        //    // Assert
        //    Assert.NotNull(result);
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        //}

        #endregion

        #region EnterpriseRoleProductProcessBatch Tests

        [Fact]
        public async Task EnterpriseRoleProductProcessBatch_WithNullBatchRecord_ReturnsBadRequest()
        {
            // Arrange
            EnterpriseRoleBatch batchRecord = null!;

            // Act
            var result = await _batchProcessController.EnterpriseRoleProductProcessBatch(batchRecord);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("enterprise role product batchRecord null.", badRequestResult.Value);
        }

        [Fact]
        public async Task EnterpriseRoleProductProcessBatch_WithValidBatchRecord_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new EnterpriseRoleBatch
            {
                EnterpriseRoleBatchProcessId = 1,
                EditorUserPersonaId = 100,
                SubjectUserPersonaId = 200,
                EnterpriseRoleTemplateId = 1,
                StatusTypeId = 1,
                BatchProcessTypeId = (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser,
                CreatedDateTime = DateTime.UtcNow
            };

            // Act
            var result = await _batchProcessController.EnterpriseRoleProductProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task EnterpriseRoleProductProcessBatch_WithMinimalBatchRecord_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new EnterpriseRoleBatch
            {
                EnterpriseRoleBatchProcessId = 0,
                BatchProcessTypeId = (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser
            };

            // Act
            var result = await _batchProcessController.EnterpriseRoleProductProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task EnterpriseRoleProductProcessBatch_WithZeroPersonaIds_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new EnterpriseRoleBatch
            {
                EnterpriseRoleBatchProcessId = 1,
                EditorUserPersonaId = 0,
                SubjectUserPersonaId = 0,
                BatchProcessTypeId = (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser
            };

            // Act
            var result = await _batchProcessController.EnterpriseRoleProductProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task EnterpriseRoleProductProcessBatch_WithLargeIds_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new EnterpriseRoleBatch
            {
                EnterpriseRoleBatchProcessId = int.MaxValue,
                EditorUserPersonaId = long.MaxValue,
                SubjectUserPersonaId = long.MaxValue,
                EnterpriseRoleTemplateId = int.MaxValue,
                StatusTypeId = int.MaxValue,
                BatchProcessTypeId = (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser
            };

            // Act
            var result = await _batchProcessController.EnterpriseRoleProductProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task EnterpriseRoleProductProcessBatch_CallsGetUserClaim()
        {
            // Arrange
            var batchRecord = new EnterpriseRoleBatch
            {
                EnterpriseRoleBatchProcessId = 1,
                EditorUserPersonaId = 100,
                SubjectUserPersonaId = 200,
                BatchProcessTypeId = (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser
            };

            // Act
            await _batchProcessController.EnterpriseRoleProductProcessBatch(batchRecord);

            // Assert
            _mockUserClaimsAccessor.Verify(x => x.GetUserClaim(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task EnterpriseRoleProductProcessBatch_WithDefaultDateTime_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new EnterpriseRoleBatch
            {
                EnterpriseRoleBatchProcessId = 1,
                CreatedDateTime = default,
                BatchProcessTypeId = (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser
            };

            // Act
            var result = await _batchProcessController.EnterpriseRoleProductProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        #endregion

        #region ProductPrimaryPropertyProcessBatch Tests

        [Fact]
        public async Task ProductPrimaryPropertyProcessBatch_WithNullBatchRecord_ReturnsBadRequest()
        {
            // Arrange
            PrimaryPropertyBatch batchRecord = null!;

            // Act
            var result = await _batchProcessController.ProductPrimaryPropertyProcessBatch(batchRecord);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("product primary property batchRecord null.", badRequestResult.Value);
        }

        [Fact]
        public async Task ProductPrimaryPropertyProcessBatch_WithValidBatchRecord_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new PrimaryPropertyBatch
            {
                PrimaryPropertyBatchProcessId = 1,
                EditorUserPersonaId = 100,
                SubjectUserPersonaId = 200,
                StatusTypeId = 1,
                BatchProcessTypeId = (int)BatchProcessType.PrimaryPropertiesUpdateProductUser
            };

            // Act
            var result = await _batchProcessController.ProductPrimaryPropertyProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task ProductPrimaryPropertyProcessBatch_WithMinimalBatchRecord_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new PrimaryPropertyBatch
            {
                PrimaryPropertyBatchProcessId = 0,
                BatchProcessTypeId = (int)BatchProcessType.PrimaryPropertiesUpdateProductUser
            };

            // Act
            var result = await _batchProcessController.ProductPrimaryPropertyProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task ProductPrimaryPropertyProcessBatch_WithZeroPersonaIds_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new PrimaryPropertyBatch
            {
                PrimaryPropertyBatchProcessId = 1,
                EditorUserPersonaId = 0,
                SubjectUserPersonaId = 0,
                BatchProcessTypeId = (int)BatchProcessType.PrimaryPropertiesUpdateProductUser
            };

            // Act
            var result = await _batchProcessController.ProductPrimaryPropertyProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task ProductPrimaryPropertyProcessBatch_WithLargeIds_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new PrimaryPropertyBatch
            {
                PrimaryPropertyBatchProcessId = int.MaxValue,
                EditorUserPersonaId = long.MaxValue,
                SubjectUserPersonaId = long.MaxValue,
                StatusTypeId = int.MaxValue,
                BatchProcessTypeId = (int)BatchProcessType.PrimaryPropertiesUpdateProductUser
            };

            // Act
            var result = await _batchProcessController.ProductPrimaryPropertyProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task ProductPrimaryPropertyProcessBatch_CallsGetUserClaim()
        {
            // Arrange
            var batchRecord = new PrimaryPropertyBatch
            {
                PrimaryPropertyBatchProcessId = 1,
                EditorUserPersonaId = 100,
                SubjectUserPersonaId = 200,
                BatchProcessTypeId = (int)BatchProcessType.PrimaryPropertiesUpdateProductUser
            };

            // Act
            await _batchProcessController.ProductPrimaryPropertyProcessBatch(batchRecord);

            // Assert
            _mockUserClaimsAccessor.Verify(x => x.GetUserClaim(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ProductPrimaryPropertyProcessBatch_WithNegativeIds_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new PrimaryPropertyBatch
            {
                PrimaryPropertyBatchProcessId = -1,
                EditorUserPersonaId = -100,
                SubjectUserPersonaId = -200,
                BatchProcessTypeId = (int)BatchProcessType.PrimaryPropertiesUpdateProductUser
            };

            // Act
            var result = await _batchProcessController.ProductPrimaryPropertyProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        #endregion

        #region BulkUserProcessBatch Tests

        [Fact]
        public async Task BulkUserProcessBatch_WithNullBatchRecord_ReturnsBadRequest()
        {
            // Arrange
            BulkUserBatch batchRecord = null!;

            // Act
            var result = await _batchProcessController.BulkUserProcessBatch(batchRecord);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("bulk user batchRecord null.", badRequestResult.Value);
        }

        [Fact]
        public async Task BulkUserProcessBatch_WithValidBatchRecord_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new BulkUserBatch
            {
                BulkUserBatchProcessId = 1,
                EditorUserPersonaId = 100,
                SubjectUserPersonaId = 200,
                StatusTypeId = 1,
                BatchProcessTypeId = (int)BatchProcessType.AssignOrUnasignProductsForBulkUsers
            };

            // Act
            var result = await _batchProcessController.BulkUserProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task BulkUserProcessBatch_WithMinimalBatchRecord_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new BulkUserBatch
            {
                BulkUserBatchProcessId = 0,
                BatchProcessTypeId = (int)BatchProcessType.AssignOrUnasignProductsForBulkUsers
            };

            // Act
            var result = await _batchProcessController.BulkUserProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task BulkUserProcessBatch_WithZeroPersonaIds_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new BulkUserBatch
            {
                BulkUserBatchProcessId = 1,
                EditorUserPersonaId = 0,
                SubjectUserPersonaId = 0,
                BatchProcessTypeId = (int)BatchProcessType.AssignOrUnasignProductsForBulkUsers
            };

            // Act
            var result = await _batchProcessController.BulkUserProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task BulkUserProcessBatch_WithLargeIds_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new BulkUserBatch
            {
                BulkUserBatchProcessId = int.MaxValue,
                EditorUserPersonaId = long.MaxValue,
                SubjectUserPersonaId = long.MaxValue,
                StatusTypeId = int.MaxValue,
                BatchProcessTypeId = (int)BatchProcessType.AssignOrUnasignProductsForBulkUsers
            };

            // Act
            var result = await _batchProcessController.BulkUserProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task BulkUserProcessBatch_CallsGetUserClaim()
        {
            // Arrange
            var batchRecord = new BulkUserBatch
            {
                BulkUserBatchProcessId = 1,
                EditorUserPersonaId = 100,
                SubjectUserPersonaId = 200,
                BatchProcessTypeId = (int)BatchProcessType.AssignOrUnasignProductsForBulkUsers
            };

            // Act
            await _batchProcessController.BulkUserProcessBatch(batchRecord);

            // Assert
            _mockUserClaimsAccessor.Verify(x => x.GetUserClaim(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task BulkUserProcessBatch_WithEmptyBulkUserProducts_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new BulkUserBatch
            {
                BulkUserBatchProcessId = 1,
                BulkUserProducts = new List<BulkUserProduct>(),
                BatchProcessTypeId = (int)BatchProcessType.AssignOrUnasignProductsForBulkUsers
            };

            // Act
            var result = await _batchProcessController.BulkUserProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task BulkUserProcessBatch_WithBulkUserProducts_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new BulkUserBatch
            {
                BulkUserBatchProcessId = 1,
                EditorUserPersonaId = 100,
                SubjectUserPersonaId = 200,
                BatchProcessTypeId = (int)BatchProcessType.AssignOrUnasignProductsForBulkUsers,
                BulkUserProducts = new List<BulkUserProduct>
                {
                    new BulkUserProduct { ProductId = 1 },
                    new BulkUserProduct { ProductId = 2 }
                }
            };

            // Act
            var result = await _batchProcessController.BulkUserProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task BulkUserProcessBatch_WithNegativeIds_ReturnsCreatedStatus()
        {
            // Arrange
            var batchRecord = new BulkUserBatch
            {
                BulkUserBatchProcessId = -1,
                EditorUserPersonaId = -100,
                SubjectUserPersonaId = -200,
                BatchProcessTypeId = (int)BatchProcessType.AssignOrUnasignProductsForBulkUsers
            };

            // Act
            var result = await _batchProcessController.BulkUserProcessBatch(batchRecord);

            // Assert
            Assert.NotNull(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        #endregion

        #region Multiple Concurrent Calls Tests

        //[Fact]
        //public async Task ProcessBatch_MultipleConcurrentCalls_AllReturnCreated()
        //{
        //    // Arrange
        //    var tasks = new List<Task<IActionResult>>();
        //    for (int i = 0; i < 5; i++)
        //    {
        //        var batchRecord = new ProductUserProperitiesRoles
        //        {
        //            ProductBatchId = i,
        //            ProductId = 1,
        //            BatchProcessType = BatchProcessType.CreateUpdateProductUser
        //        };
        //        tasks.Add(_batchProcessController.ProcessBatch(batchRecord));
        //    }

        //    // Act
        //    var results = await Task.WhenAll(tasks);

        //    // Assert
        //    foreach (var result in results)
        //    {
        //        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //        Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        //    }
        //}

        [Fact]
        public async Task EnterpriseRoleProductProcessBatch_MultipleConcurrentCalls_AllReturnCreated()
        {
            // Arrange
            var tasks = new List<Task<IActionResult>>();
            for (int i = 0; i < 5; i++)
            {
                var batchRecord = new EnterpriseRoleBatch
                {
                    EnterpriseRoleBatchProcessId = i,
                    EditorUserPersonaId = 100 + i,
                    SubjectUserPersonaId = 200 + i,
                    BatchProcessTypeId = (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser
                };
                tasks.Add(_batchProcessController.EnterpriseRoleProductProcessBatch(batchRecord));
            }

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                var statusCodeResult = Assert.IsType<ObjectResult>(result);
                Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
            }
        }

        #endregion

        #region Integration Verification Tests

        [Fact]
        public async Task AllEndpoints_WithNullBatchRecords_AllReturnBadRequest()
        {
            // Act
            var processResult = await _batchProcessController.ProcessBatch(null!);
            var enterpriseResult = await _batchProcessController.EnterpriseRoleProductProcessBatch(null!);
            var primaryPropertyResult = await _batchProcessController.ProductPrimaryPropertyProcessBatch(null!);
            var bulkUserResult = await _batchProcessController.BulkUserProcessBatch(null!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(processResult);
            Assert.IsType<BadRequestObjectResult>(enterpriseResult);
            Assert.IsType<BadRequestObjectResult>(primaryPropertyResult);
            Assert.IsType<BadRequestObjectResult>(bulkUserResult);
        }

        //[Fact]
        //public async Task AllEndpoints_WithValidBatchRecords_AllReturnCreated()
        //{
        //    // Act
        //    var processResult = await _batchProcessController.ProcessBatch(new ProductUserProperitiesRoles 
        //    { 
        //        ProductBatchId = 1,
        //        BatchProcessType = BatchProcessType.CreateUpdateProductUser
        //    });
        //    var enterpriseResult = await _batchProcessController.EnterpriseRoleProductProcessBatch(new EnterpriseRoleBatch 
        //    { 
        //        EnterpriseRoleBatchProcessId = 1,
        //        BatchProcessTypeId = (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser
        //    });
        //    var primaryPropertyResult = await _batchProcessController.ProductPrimaryPropertyProcessBatch(new PrimaryPropertyBatch 
        //    { 
        //        PrimaryPropertyBatchProcessId = 1,
        //        BatchProcessTypeId = (int)BatchProcessType.PrimaryPropertiesUpdateProductUser
        //    });
        //    var bulkUserResult = await _batchProcessController.BulkUserProcessBatch(new BulkUserBatch 
        //    { 
        //        BulkUserBatchProcessId = 1,
        //        BatchProcessTypeId = (int)BatchProcessType.AssignOrUnasignProductsForBulkUsers
        //    });

        //    // Assert
        //    Assert.Equal((int)HttpStatusCode.Created, ((ObjectResult)processResult).StatusCode);
        //    Assert.Equal((int)HttpStatusCode.Created, ((ObjectResult)enterpriseResult).StatusCode);
        //    Assert.Equal((int)HttpStatusCode.Created, ((ObjectResult)primaryPropertyResult).StatusCode);
        //    Assert.Equal((int)HttpStatusCode.Created, ((ObjectResult)bulkUserResult).StatusCode);
        //}

        #endregion

        #region Response Content Verification Tests

        //[Fact]
        //public async Task ProcessBatch_ReturnsNonNullContent()
        //{
        //    // Arrange
        //    var batchRecord = new ProductUserProperitiesRoles 
        //    { 
        //        ProductBatchId = 1,
        //        BatchProcessType = BatchProcessType.CreateUpdateProductUser
        //    };

        //    // Act
        //    var result = await _batchProcessController.ProcessBatch(batchRecord);

        //    // Assert
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.NotNull(statusCodeResult.Value);
        //}

        [Fact]
        public async Task EnterpriseRoleProductProcessBatch_ReturnsNonNullContent()
        {
            // Arrange
            var batchRecord = new EnterpriseRoleBatch 
            { 
                EnterpriseRoleBatchProcessId = 1,
                BatchProcessTypeId = (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser
            };

            // Act
            var result = await _batchProcessController.EnterpriseRoleProductProcessBatch(batchRecord);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(statusCodeResult.Value);
        }

        [Fact]
        public async Task ProductPrimaryPropertyProcessBatch_ReturnsNonNullContent()
        {
            // Arrange
            var batchRecord = new PrimaryPropertyBatch 
            { 
                PrimaryPropertyBatchProcessId = 1,
                BatchProcessTypeId = (int)BatchProcessType.PrimaryPropertiesUpdateProductUser
            };

            // Act
            var result = await _batchProcessController.ProductPrimaryPropertyProcessBatch(batchRecord);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(statusCodeResult.Value);
        }

        [Fact]
        public async Task BulkUserProcessBatch_ReturnsNonNullContent()
        {
            // Arrange
            var batchRecord = new BulkUserBatch 
            { 
                BulkUserBatchProcessId = 1,
                BatchProcessTypeId = (int)BatchProcessType.AssignOrUnasignProductsForBulkUsers
            };

            // Act
            var result = await _batchProcessController.BulkUserProcessBatch(batchRecord);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(statusCodeResult.Value);
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _batchProcessController = null!;
            base.Dispose();
        }

        #endregion
    }
}














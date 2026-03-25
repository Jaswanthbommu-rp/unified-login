using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
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

        private readonly Mock<IBatchProcessServiceAsync> _mockBatchProcessService;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private BatchProcessController _batchProcessController;

        #endregion

        #region Constructor

        public BatchProcessControllerTests()
        {
            _mockBatchProcessService = new Mock<IBatchProcessServiceAsync>();
            _mockUserClaimsAccessor = MockUserClaimsAccessor;

            _batchProcessController = new BatchProcessController(
                _mockBatchProcessService.Object,
                _mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullBatchProcessService_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new BatchProcessController(null!, _mockUserClaimsAccessor.Object));

            Assert.Equal("batchProcessService", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new BatchProcessController(_mockBatchProcessService.Object, null!));

            Assert.Equal("userClaimsAccessor", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new BatchProcessController(
                _mockBatchProcessService.Object,
                _mockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        #endregion

        #region ProcessBatch Tests

        [Fact]
        public async Task ProcessBatch_WithNullBatchRecord_ReturnsBadRequest()
        {
            var result = await _batchProcessController.ProcessBatch(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("batchRecord is null.", badRequestResult.Value);
        }

        [Fact]
        public async Task ProcessBatch_WithValidBatchRecord_ReturnsCreatedStatus()
        {
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
                BatchProcessorGroupId = 1
            };

            _mockBatchProcessService
                .Setup(x => x.ProcessBatchAsync(batchRecord, It.IsAny<CancellationToken>()))
                .ReturnsAsync("Processed");

            var result = await _batchProcessController.ProcessBatch(batchRecord);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task ProcessBatch_WhenServiceReturnsEmptyString_ReturnsSuccess()
        {
            var batchRecord = new ProductUserProperitiesRoles
            {
                ProductBatchId = 1,
                BatchProcessType = BatchProcessType.ProfileUpdate
            };

            _mockBatchProcessService
                .Setup(x => x.ProcessBatchAsync(batchRecord, It.IsAny<CancellationToken>()))
                .ReturnsAsync(string.Empty);

            var result = await _batchProcessController.ProcessBatch(batchRecord);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
            Assert.Equal("Success", statusCodeResult.Value);
        }

        [Fact]
        public async Task ProcessBatch_WithProfileUpdateType_ReturnsCreatedStatus()
        {
            var batchRecord = new ProductUserProperitiesRoles
            {
                ProductBatchId = 1,
                BatchProcessType = BatchProcessType.ProfileUpdate
            };

            _mockBatchProcessService
                .Setup(x => x.ProcessBatchAsync(batchRecord, It.IsAny<CancellationToken>()))
                .ReturnsAsync("Processed");

            var result = await _batchProcessController.ProcessBatch(batchRecord);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        #endregion

        #region EnterpriseRoleProductProcessBatch Tests

        [Fact]
        public async Task EnterpriseRoleProductProcessBatch_WithNullBatchRecord_ReturnsBadRequest()
        {
            var result = await _batchProcessController.EnterpriseRoleProductProcessBatch(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("enterprise role product batchRecord null.", badRequestResult.Value);
        }

        [Fact]
        public async Task EnterpriseRoleProductProcessBatch_WithValidBatchRecord_ReturnsCreatedStatus()
        {
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

            _mockBatchProcessService
                .Setup(x => x.ProcessEnterpriseRoleProductBatchAsync(batchRecord, It.IsAny<DefaultUserClaim>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Processed");

            var result = await _batchProcessController.EnterpriseRoleProductProcessBatch(batchRecord);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task EnterpriseRoleProductProcessBatch_WhenServiceReturnsEmptyString_ReturnsSuccess()
        {
            var batchRecord = new EnterpriseRoleBatch
            {
                EnterpriseRoleBatchProcessId = 1,
                BatchProcessTypeId = (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser
            };

            _mockBatchProcessService
                .Setup(x => x.ProcessEnterpriseRoleProductBatchAsync(batchRecord, It.IsAny<DefaultUserClaim>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(string.Empty);

            var result = await _batchProcessController.EnterpriseRoleProductProcessBatch(batchRecord);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
            Assert.Equal("Success", statusCodeResult.Value);
        }

        [Fact]
        public async Task EnterpriseRoleProductProcessBatch_CallsGetUserClaim()
        {
            var batchRecord = new EnterpriseRoleBatch
            {
                EnterpriseRoleBatchProcessId = 1,
                BatchProcessTypeId = (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser
            };

            _mockBatchProcessService
                .Setup(x => x.ProcessEnterpriseRoleProductBatchAsync(batchRecord, It.IsAny<DefaultUserClaim>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Processed");

            await _batchProcessController.EnterpriseRoleProductProcessBatch(batchRecord);

            _mockUserClaimsAccessor.Verify(x => x.GetUserClaim(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task EnterpriseRoleProductProcessBatch_ReturnsNonNullContent()
        {
            var batchRecord = new EnterpriseRoleBatch
            {
                EnterpriseRoleBatchProcessId = 1,
                BatchProcessTypeId = (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser
            };

            _mockBatchProcessService
                .Setup(x => x.ProcessEnterpriseRoleProductBatchAsync(batchRecord, It.IsAny<DefaultUserClaim>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Processed");

            var result = await _batchProcessController.EnterpriseRoleProductProcessBatch(batchRecord);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(statusCodeResult.Value);
        }

        [Fact]
        public async Task EnterpriseRoleProductProcessBatch_MultipleConcurrentCalls_AllReturnCreated()
        {
            var tasks = new List<Task<IActionResult>>();
            for (int i = 0; i < 5; i++)
            {
                var batchRecord = new EnterpriseRoleBatch
                {
                    EnterpriseRoleBatchProcessId = i,
                    BatchProcessTypeId = (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser
                };

                _mockBatchProcessService
                    .Setup(x => x.ProcessEnterpriseRoleProductBatchAsync(batchRecord, It.IsAny<DefaultUserClaim>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync("Processed");

                tasks.Add(_batchProcessController.EnterpriseRoleProductProcessBatch(batchRecord));
            }

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                var statusCodeResult = Assert.IsType<ObjectResult>(result);
                Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
            }
        }

        #endregion

        #region ProductPrimaryPropertyProcessBatch Tests

        [Fact]
        public async Task ProductPrimaryPropertyProcessBatch_WithNullBatchRecord_ReturnsBadRequest()
        {
            var result = await _batchProcessController.ProductPrimaryPropertyProcessBatch(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("product primary property batchRecord null.", badRequestResult.Value);
        }

        [Fact]
        public async Task ProductPrimaryPropertyProcessBatch_WithValidBatchRecord_ReturnsCreatedStatus()
        {
            var batchRecord = new PrimaryPropertyBatch
            {
                PrimaryPropertyBatchProcessId = 1,
                EditorUserPersonaId = 100,
                SubjectUserPersonaId = 200,
                StatusTypeId = 1,
                BatchProcessTypeId = (int)BatchProcessType.PrimaryPropertiesUpdateProductUser
            };

            _mockBatchProcessService
                .Setup(x => x.ProcessPrimaryPropertyBatchAsync(batchRecord, It.IsAny<DefaultUserClaim>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Processed");

            var result = await _batchProcessController.ProductPrimaryPropertyProcessBatch(batchRecord);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task ProductPrimaryPropertyProcessBatch_WhenServiceReturnsEmptyString_ReturnsSuccess()
        {
            var batchRecord = new PrimaryPropertyBatch
            {
                PrimaryPropertyBatchProcessId = 1,
                BatchProcessTypeId = (int)BatchProcessType.PrimaryPropertiesUpdateProductUser
            };

            _mockBatchProcessService
                .Setup(x => x.ProcessPrimaryPropertyBatchAsync(batchRecord, It.IsAny<DefaultUserClaim>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(string.Empty);

            var result = await _batchProcessController.ProductPrimaryPropertyProcessBatch(batchRecord);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
            Assert.Equal("Success", statusCodeResult.Value);
        }

        [Fact]
        public async Task ProductPrimaryPropertyProcessBatch_CallsGetUserClaim()
        {
            var batchRecord = new PrimaryPropertyBatch
            {
                PrimaryPropertyBatchProcessId = 1,
                BatchProcessTypeId = (int)BatchProcessType.PrimaryPropertiesUpdateProductUser
            };

            _mockBatchProcessService
                .Setup(x => x.ProcessPrimaryPropertyBatchAsync(batchRecord, It.IsAny<DefaultUserClaim>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Processed");

            await _batchProcessController.ProductPrimaryPropertyProcessBatch(batchRecord);

            _mockUserClaimsAccessor.Verify(x => x.GetUserClaim(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ProductPrimaryPropertyProcessBatch_ReturnsNonNullContent()
        {
            var batchRecord = new PrimaryPropertyBatch
            {
                PrimaryPropertyBatchProcessId = 1,
                BatchProcessTypeId = (int)BatchProcessType.PrimaryPropertiesUpdateProductUser
            };

            _mockBatchProcessService
                .Setup(x => x.ProcessPrimaryPropertyBatchAsync(batchRecord, It.IsAny<DefaultUserClaim>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Processed");

            var result = await _batchProcessController.ProductPrimaryPropertyProcessBatch(batchRecord);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(statusCodeResult.Value);
        }

        #endregion

        #region BulkUserProcessBatch Tests

        [Fact]
        public async Task BulkUserProcessBatch_WithNullBatchRecord_ReturnsBadRequest()
        {
            var result = await _batchProcessController.BulkUserProcessBatch(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("bulk user batchRecord null.", badRequestResult.Value);
        }

        [Fact]
        public async Task BulkUserProcessBatch_WithValidBatchRecord_ReturnsCreatedStatus()
        {
            var batchRecord = new BulkUserBatch
            {
                BulkUserBatchProcessId = 1,
                EditorUserPersonaId = 100,
                SubjectUserPersonaId = 200,
                StatusTypeId = 1,
                BatchProcessTypeId = (int)BatchProcessType.AssignOrUnasignProductsForBulkUsers
            };

            _mockBatchProcessService
                .Setup(x => x.ProcessBulkUserBatchAsync(batchRecord, It.IsAny<DefaultUserClaim>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Processed");

            var result = await _batchProcessController.BulkUserProcessBatch(batchRecord);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task BulkUserProcessBatch_WhenServiceReturnsEmptyString_ReturnsSuccess()
        {
            var batchRecord = new BulkUserBatch
            {
                BulkUserBatchProcessId = 1,
                BatchProcessTypeId = (int)BatchProcessType.AssignOrUnasignProductsForBulkUsers
            };

            _mockBatchProcessService
                .Setup(x => x.ProcessBulkUserBatchAsync(batchRecord, It.IsAny<DefaultUserClaim>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(string.Empty);

            var result = await _batchProcessController.BulkUserProcessBatch(batchRecord);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
            Assert.Equal("Success", statusCodeResult.Value);
        }

        [Fact]
        public async Task BulkUserProcessBatch_CallsGetUserClaim()
        {
            var batchRecord = new BulkUserBatch
            {
                BulkUserBatchProcessId = 1,
                BatchProcessTypeId = (int)BatchProcessType.AssignOrUnasignProductsForBulkUsers
            };

            _mockBatchProcessService
                .Setup(x => x.ProcessBulkUserBatchAsync(batchRecord, It.IsAny<DefaultUserClaim>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Processed");

            await _batchProcessController.BulkUserProcessBatch(batchRecord);

            _mockUserClaimsAccessor.Verify(x => x.GetUserClaim(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task BulkUserProcessBatch_WithBulkUserProducts_ReturnsCreatedStatus()
        {
            var batchRecord = new BulkUserBatch
            {
                BulkUserBatchProcessId = 1,
                BatchProcessTypeId = (int)BatchProcessType.AssignOrUnasignProductsForBulkUsers,
                BulkUserProducts = new List<BulkUserProduct>
                {
                    new BulkUserProduct { ProductId = 1 },
                    new BulkUserProduct { ProductId = 2 }
                }
            };

            _mockBatchProcessService
                .Setup(x => x.ProcessBulkUserBatchAsync(batchRecord, It.IsAny<DefaultUserClaim>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Processed");

            var result = await _batchProcessController.BulkUserProcessBatch(batchRecord);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task BulkUserProcessBatch_ReturnsNonNullContent()
        {
            var batchRecord = new BulkUserBatch
            {
                BulkUserBatchProcessId = 1,
                BatchProcessTypeId = (int)BatchProcessType.AssignOrUnasignProductsForBulkUsers
            };

            _mockBatchProcessService
                .Setup(x => x.ProcessBulkUserBatchAsync(batchRecord, It.IsAny<DefaultUserClaim>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Processed");

            var result = await _batchProcessController.BulkUserProcessBatch(batchRecord);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(statusCodeResult.Value);
        }

        #endregion

        #region AllEndpoints Tests

        [Fact]
        public async Task AllEndpoints_WithNullBatchRecords_AllReturnBadRequest()
        {
            var processResult = await _batchProcessController.ProcessBatch(null!);
            var enterpriseResult = await _batchProcessController.EnterpriseRoleProductProcessBatch(null!);
            var primaryPropertyResult = await _batchProcessController.ProductPrimaryPropertyProcessBatch(null!);
            var bulkUserResult = await _batchProcessController.BulkUserProcessBatch(null!);

            Assert.IsType<BadRequestObjectResult>(processResult);
            Assert.IsType<BadRequestObjectResult>(enterpriseResult);
            Assert.IsType<BadRequestObjectResult>(primaryPropertyResult);
            Assert.IsType<BadRequestObjectResult>(bulkUserResult);
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

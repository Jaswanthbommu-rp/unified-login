namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Constants
{
	internal static class StoredProcNameConstants
	{
		internal const string SP_ListBatchConfiguration = "Batch.ListBatchProcessConfiguration";
		internal const string SP_ListBatch = "Batch.ListBatchProcessor";
		internal const string SP_UpdateBatch = "Enterprise.UpdateProductBatch";
		internal const string SP_EnterpriseRoleListBatch = "Batch.ListEnterpriseRoleBatchProcessor";
		internal const string SP_UpdateEnterpriseRoleProductBatch = "Batch.UpdateEnterpriseRoleProductBatch";
		internal const string SP_PrimaryPropertiesBatch = "Batch.ListPrimaryPropertiesBatchProcessor";
		internal const string SP_UpdatePrimaryPropertyProductBatch = "Batch.UpdatePrimaryPropertiesProductBatch";
		internal const string SP_ListGlobalSettingsForProduct = "Enterprise.ListGlobalSettingsForProduct";
        internal const string SP_BulkUserBatch = "Batch.ListBulkUserBatchProcessor";
        internal const string SP_UpdateBulkUserBatch = "Batch.UpdateBulkUserProductBatch";
    }
}

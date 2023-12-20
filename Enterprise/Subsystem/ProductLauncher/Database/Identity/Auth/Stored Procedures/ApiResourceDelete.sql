CREATE PROCEDURE [Auth].[ApiResourceDelete]
(
	@Original_Id int,
	@Original_Enabled bit,
	@Original_Name nvarchar(200),
	@IsNull_DisplayName Int,
	@Original_DisplayName nvarchar(200),
	@IsNull_Description Int,
	@Original_Description nvarchar(1000),
	@IsNull_AllowedAccessTokenSigningAlgorithms Int,
	@Original_AllowedAccessTokenSigningAlgorithms nvarchar(100),
	@Original_ShowInDiscoveryDocument BIT,
	@Original_RequireResourceIndicator bit
)
AS
BEGIN
	SET NOCOUNT OFF;
	DELETE FROM [Auth].[ApiResources] 
	WHERE 
		(([Id] = @Original_Id) 
		AND ([Enabled] = @Original_Enabled) 
		AND ([Name] = @Original_Name) 
		AND ((@IsNull_DisplayName = 1 AND [DisplayName] IS NULL) OR ([DisplayName] = @Original_DisplayName)) 
		AND ((@IsNull_Description = 1 AND [Description] IS NULL) OR ([Description] = @Original_Description)) 
		AND ((@IsNull_AllowedAccessTokenSigningAlgorithms = 1 AND [AllowedAccessTokenSigningAlgorithms] IS NULL) OR ([AllowedAccessTokenSigningAlgorithms] = @Original_AllowedAccessTokenSigningAlgorithms))
		AND ([ShowInDiscoveryDocument] = @Original_ShowInDiscoveryDocument) 
		AND ([RequireResourceIndicator] = @Original_RequireResourceIndicator))

    SELECT @@RowCount [RowsAffected]
END
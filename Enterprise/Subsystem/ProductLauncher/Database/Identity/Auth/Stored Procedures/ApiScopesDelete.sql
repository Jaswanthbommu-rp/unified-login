CREATE PROCEDURE [Auth].[ApiScopesDelete]
(
	@Original_Id int,
	@Original_Enabled bit,
	@Original_Name nvarchar(200),
	@IsNull_DisplayName Int,
	@Original_DisplayName nvarchar(200),
	@IsNull_Description Int,
	@Original_Description nvarchar(1000),
	@Original_Required bit,
	@Original_Emphasize bit,
	@Original_ShowInDiscoveryDocument bit,
	@Original_NonEditable bit
)
AS
BEGIN
	SET NOCOUNT OFF;
	
	IF EXISTS ( SELECT TOP 1(1) FROM Auth.ApiResourceScopes WHERE Scope = @Original_Name )
	BEGIN
		SELECT 0 [RowsAffected]
		RETURN 0
	END
	
	DELETE FROM [Auth].[ApiScopes] 
	WHERE 
		(([Id] = @Original_Id) 
		AND ([Enabled] = @Original_Enabled) 
		AND ([Name] = @Original_Name) 
		AND ((@IsNull_DisplayName = 1 
		AND [DisplayName] IS NULL) OR ([DisplayName] = @Original_DisplayName)) 
		AND ((@IsNull_Description = 1 AND [Description] IS NULL) OR ([Description] = @Original_Description)) 
		AND ([Required] = @Original_Required) 
		AND ([Emphasize] = @Original_Emphasize) 
		AND ([ShowInDiscoveryDocument] = @Original_ShowInDiscoveryDocument) 
		AND ([NonEditable] = @Original_NonEditable))

    SELECT @@RowCount [RowsAffected]
END

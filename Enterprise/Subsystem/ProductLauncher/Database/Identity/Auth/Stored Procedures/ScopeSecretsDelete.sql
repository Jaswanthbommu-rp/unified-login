CREATE PROCEDURE [Auth].ScopeSecretsDelete
(
	@Original_ScopeSecretId int,
	@Original_ScopeId int,
	@IsNull_Description Int,
	@Original_Description nvarchar(1000),
	@IsNull_Type Int,
	@Original_Type nvarchar(250),
	@Original_Value nvarchar(250),
	@IsNull_Expiration Int,
	@Original_Expiration datetimeoffset
)
AS
BEGIN
	SET NOCOUNT OFF;
	DELETE FROM [Auth].[ScopeSecrets] 
	WHERE 
		(([ScopeSecretId] = @Original_ScopeSecretId) 
		AND ([ScopeId] = @Original_ScopeId) 
		AND ((@IsNull_Description = 1 AND [Description] IS NULL) OR ([Description] = @Original_Description)) 
		AND ((@IsNull_Type = 1 AND [Type] IS NULL) OR ([Type] = @Original_Type)) 
		AND ([Value] = @Original_Value) 
		AND ((@IsNull_Expiration = 1 
		AND [Expiration] IS NULL) OR ([Expiration] = @Original_Expiration)))
END
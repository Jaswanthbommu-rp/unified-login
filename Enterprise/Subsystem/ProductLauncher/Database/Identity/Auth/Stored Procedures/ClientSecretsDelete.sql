CREATE PROCEDURE [Auth].ClientSecretsDelete
(
	@Original_ClientSecretId int,
	@Original_ClientId int,
	@Original_Value nvarchar(250),
	@IsNull_Type Int,
	@Original_Type nvarchar(250),
	@IsNull_Description Int,
	@Original_Description nvarchar(2000),
	@IsNull_Expiration Int,
	@Original_Expiration datetimeoffset
)
AS
BEGIN
	SET NOCOUNT OFF;
	DELETE FROM [Auth].[ClientSecrets] 
	WHERE 
		(([Id] = @Original_ClientSecretId) 
		AND ([ClientId] = @Original_ClientId) 
		AND ([Value] = @Original_Value) 
		AND ((@IsNull_Type = 1 AND [Type] IS NULL) OR ([Type] = @Original_Type)) 
		AND ((@IsNull_Description = 1 AND [Description] IS NULL) OR ([Description] = @Original_Description)) 
		AND ((@IsNull_Expiration = 1 AND [Expiration] IS NULL) OR ([Expiration] = @Original_Expiration)))

	SELECT @@RowCount [RowsAffected]
END
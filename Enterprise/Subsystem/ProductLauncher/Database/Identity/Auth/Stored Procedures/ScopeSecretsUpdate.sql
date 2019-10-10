CREATE PROCEDURE [Auth].ScopeSecretsUpdate
(
	@ScopeId int,
	@Description nvarchar(1000),
	@Type nvarchar(250),
	@Value nvarchar(250),
	@Expiration datetimeoffset,
	@Original_ScopeSecretId int,
	@Original_ScopeId int,
	@IsNull_Description Int,
	@Original_Description nvarchar(1000),
	@IsNull_Type Int,
	@Original_Type nvarchar(250),
	@Original_Value nvarchar(250),
	@IsNull_Expiration Int,
	@Original_Expiration datetimeoffset,
	@ScopeSecretId int
)
AS
BEGIN
	SET NOCOUNT OFF;
	UPDATE [Auth].[ScopeSecrets] 
	SET 
		[ScopeId] = @ScopeId
		, [Description] = @Description
		, [Type] = @Type
		, [Value] = @Value
		, [Expiration] = @Expiration 
	WHERE 
		(([ScopeSecretId] = @Original_ScopeSecretId) 
		AND ([ScopeId] = @Original_ScopeId) 
		AND ((@IsNull_Description = 1 AND [Description] IS NULL) OR ([Description] = @Original_Description)) 
		AND ((@IsNull_Type = 1 AND [Type] IS NULL) OR ([Type] = @Original_Type)) 
		AND ([Value] = @Original_Value) 
		AND ((@IsNull_Expiration = 1 AND [Expiration] IS NULL) OR ([Expiration] = @Original_Expiration)));
	
	SELECT       
		ScopeSecretId as Id
		, ScopeId
		, Description
		, Type
		, Value
		, Expiration
	FROM
		Auth.ScopeSecrets  
	WHERE (ScopeSecretId = @ScopeSecretId)
END

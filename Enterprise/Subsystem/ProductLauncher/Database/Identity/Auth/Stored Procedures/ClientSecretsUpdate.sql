CREATE PROCEDURE [Auth].ClientSecretsUpdate
(
	@ClientId int,
	@Value nvarchar(250),
	@Type nvarchar(250),
	@Description nvarchar(2000),
	@Expiration datetimeoffset,
	@Original_ClientSecretId int,
	@Original_ClientId int,
	@Original_Value nvarchar(250),
	@IsNull_Type Int,
	@Original_Type nvarchar(250),
	@IsNull_Description Int,
	@Original_Description nvarchar(2000),
	@IsNull_Expiration Int,
	@Original_Expiration datetimeoffset,
	@ClientSecretId int
)
AS
BEGIN
	SET NOCOUNT OFF;
	UPDATE [Auth].[ClientSecrets] 
	SET 
		[ClientId] = @ClientId
		, [Value] = @Value
		, [Type] = @Type
		, [Description] = @Description
		, [Expiration] = @Expiration 
	WHERE 
		(([Id] = @Original_ClientSecretId) 
		AND ([ClientId] = @Original_ClientId) 
		AND ([Value] = @Original_Value) 
		AND ((@IsNull_Type = 1 AND [Type] IS NULL) OR ([Type] = @Original_Type)) 
		AND ((@IsNull_Description = 1 AND [Description] IS NULL) OR ([Description] = @Original_Description)) 
		AND ((@IsNull_Expiration = 1 AND [Expiration] IS NULL) OR ([Expiration] = @Original_Expiration)));
	
	SELECT
		Id
		, ClientId
		, Value
		, Type
		, Description
		, Created
		, Expiration 
	FROM Auth.ClientSecrets 
	WHERE 
		(Id = @ClientSecretId)
END

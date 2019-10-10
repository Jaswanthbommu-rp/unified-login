CREATE PROCEDURE [Auth].ScopeSecretsInsert
(
	@ScopeId int,
	@Description nvarchar(1000),
	@Type nvarchar(250),
	@Value nvarchar(250),
	@Expiration datetimeoffset
)
AS
BEGIN
	SET NOCOUNT OFF;
	INSERT INTO [Auth].[ScopeSecrets] 
		([ScopeId]
		, [Description]
		, [Type]
		, [Value]
		, [Expiration]) 
	VALUES 
		(@ScopeId
		, @Description
		, @Type
		, @Value
		, @Expiration);
	
	SELECT       
		ScopeSecretId as Id
		, ScopeId
		, Description
		, Type
		, Value
		, Expiration
	FROM
		Auth.ScopeSecrets 
	WHERE (ScopeSecretId = SCOPE_IDENTITY())
END
GO

CREATE PROCEDURE [Auth].ClientSecretsInsert
(
	@ClientId int,
	@Value nvarchar(250),
	@Type nvarchar(250),
	@Description nvarchar(2000),
	@Expiration datetimeoffset
)
AS
BEGIN
	SET NOCOUNT OFF;
	INSERT INTO [Auth].[ClientSecrets] 
		([ClientId]
		, [Value]
		, [Type]
		, [Description]
		, [Expiration]
		, [Created]
		) 
	VALUES 
		(@ClientId
		, @Value
		, @Type
		, @Description
		, @Expiration
		, GETUTCDATE()	
	);
	
	SELECT 
		 Id
		, ClientId
		, Value
		, Type
		, Description
		, Expiration 
	FROM Auth.ClientSecrets 
	WHERE 
		(Id = SCOPE_IDENTITY())
END

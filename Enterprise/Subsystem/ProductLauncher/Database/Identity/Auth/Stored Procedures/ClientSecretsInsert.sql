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
		, [Expiration]) 
	VALUES 
		(@ClientId
		, @Value
		, @Type
		, @Description
		, @Expiration);
	
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
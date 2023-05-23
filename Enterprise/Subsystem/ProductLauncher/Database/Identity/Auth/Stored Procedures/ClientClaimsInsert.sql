CREATE PROCEDURE [Auth].ClientClaimsInsert
(
	@ClientId int,
	@Type nvarchar(100),
	@Value nvarchar(100)
)
AS
BEGIN
	SET NOCOUNT OFF;
	INSERT INTO [Auth].[ClientClaims] 
		([ClientId]
		, [Type]
		, [Value]) 
	VALUES 
		(@ClientId
		, @Type
		, @Value);
	
	SELECT 
		  Id
		, ClientId
		, Type
		, Value 
	FROM 
		Auth.ClientClaims 
	WHERE 
		(Id = SCOPE_IDENTITY())
END

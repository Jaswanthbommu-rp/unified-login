CREATE PROCEDURE [Auth].ClientScopesInsert
(
	@ClientId int,
	@Scope nvarchar(200)
)
AS
BEGIN
	SET NOCOUNT OFF;
	INSERT INTO [Auth].[ClientScopes] 
		([ClientId]
		, [Scope]) 
	VALUES 
		(@ClientId
		, @Scope);
	
	SELECT 
		  Id
		, ClientId
		, Scope 
	FROM Auth.ClientScopes 
	WHERE 
		(Id = SCOPE_IDENTITY())
END
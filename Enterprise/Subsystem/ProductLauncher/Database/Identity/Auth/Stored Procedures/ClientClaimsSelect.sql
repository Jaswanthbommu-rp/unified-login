CREATE PROCEDURE [Auth].ClientClaimsSelect
(
	@ClientId INT = 0
)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT       Id
				,ClientId
				,Type
				,Value 
	FROM            
		Auth.ClientClaims
	WHERE
		@ClientId = 0 OR ClientId = @ClientId
END
GO
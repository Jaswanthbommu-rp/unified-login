CREATE PROCEDURE [Auth].ClientClaimsSelect
AS
BEGIN
	SET NOCOUNT ON;
	SELECT       Id
				,ClientId
				,Type
				,Value 
	FROM            
	Auth.ClientClaims
END
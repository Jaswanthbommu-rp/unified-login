CREATE PROCEDURE [Auth].ClientClaimsSelect
AS
BEGIN
	SET NOCOUNT ON;
	SELECT      ClientClaimsId as Id
				,ClientId
				,Type
				,Value 
	FROM            
	Auth.ClientClaims
END
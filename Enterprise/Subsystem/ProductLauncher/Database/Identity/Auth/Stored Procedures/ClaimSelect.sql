CREATE PROCEDURE [Auth].[ClaimSelect]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT        
		  ClaimId
		, ClaimName
		, SAMLAttributeName
		, ProductId
	FROM 
		Auth.Claim
END
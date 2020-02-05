CREATE PROCEDURE [Auth].[ClientUserClaimSelect]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT        
		  ClientUserClaimId
		, ClientId
		, ClaimId
	FROM  
		Auth.ClientUserClaim
END

CREATE PROCEDURE [Auth].[GetUserClaimTypesRequiredForClient] (
	 @ClientName VARCHAR(50) = ''  -- input param
)
AS
BEGIN
	SET NOCOUNT ON; 

	
	SELECT CL.ClaimName
		  ,CL.SAMLAttributeName
		  ,CL.ProductId 
	FROM Auth.Clients C
	INNER JOIN Auth.ClientUserClaim AS cc ON C.Id = CC.ClientId
	INNER JOIN Auth.Claim CL ON CL.ClaimId = CC.ClaimId
	WHERE C.ClientId= @ClientName
END
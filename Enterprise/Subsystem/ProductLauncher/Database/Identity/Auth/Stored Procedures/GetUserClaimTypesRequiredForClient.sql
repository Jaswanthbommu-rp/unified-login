CREATE PROCEDURE [Auth].[GetUserClaimTypesRequiredForClient] (
	 @ClientName VARCHAR(50) = 'unifiedAmenities'  -- input param
)
AS
BEGIN
	SET NOCOUNT ON; 

	
	SELECT CL.ClaimName
		  ,CL.SAMLAttributeName
		  ,CL.ProductId 
	FROM Auth.Client C
	INNER JOIN Auth.ClientUserClaim AS cc ON C.ClientId = CC.ClientId
	INNER JOIN Auth.Claim CL ON CL.ClaimId = CC.ClaimId
	WHERE C.ClientCode= @ClientName
END
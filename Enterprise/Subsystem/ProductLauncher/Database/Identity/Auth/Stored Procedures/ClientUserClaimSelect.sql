CREATE PROCEDURE [Auth].[ClientUserClaimSelect]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT        
		  ClientUserClaimId
		, ClientId
		,CUC. ClaimId
		,CL.ClaimName
		,CL.SAMLAttributeName
		,P.Name AS 'ProductName'
	FROM  Auth.ClientUserClaim CUC
	JOIN Auth.Claim CL ON CL.ClaimId = CUC.ClaimId
	JOIN Enterprise.Product P ON p.ProductId = CL.ProductId
END

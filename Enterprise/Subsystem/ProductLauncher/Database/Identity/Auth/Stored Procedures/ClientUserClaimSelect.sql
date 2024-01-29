CREATE PROCEDURE [Auth].[ClientUserClaimSelect]
(
	@ClientId INT = 0
)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT        
		  CUC.ClientUserClaimId
		, CUC.ClientId
		,CUC. ClaimId
		,CL.ClaimName
		,CL.SAMLAttributeName
		,P.Name AS [ProductName]
	FROM  Auth.ClientUserClaim CUC
		JOIN Auth.Claim CL ON CL.ClaimId = CUC.ClaimId
		JOIN Enterprise.Product P ON p.ProductId = CL.ProductId
	WHERE
		@ClientId = 0 OR CUC.ClientId = @ClientId
END
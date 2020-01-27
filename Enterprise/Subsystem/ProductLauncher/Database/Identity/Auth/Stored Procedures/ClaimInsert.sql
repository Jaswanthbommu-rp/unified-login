CREATE PROCEDURE [Auth].[ClaimInsert]
(
	@ClaimName nvarchar(255),
	@SAMLAttributeName nvarchar(50),
	@ProductId int
)
AS
BEGIN
	SET NOCOUNT OFF;
	INSERT INTO [Auth].[Claim] 
		([ClaimName], [SAMLAttributeName], [ProductId])
	
	VALUES 
		(@ClaimName, @SAMLAttributeName, @ProductId);
	
	SELECT 
		  ClaimId
		, ClaimName
		, SAMLAttributeName
		, ProductId 
	FROM Auth.Claim 
		WHERE 
		(ClaimId = SCOPE_IDENTITY())
END
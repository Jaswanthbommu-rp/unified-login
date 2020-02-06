CREATE PROCEDURE [Auth].[ClaimUpdate]
(
	@ClaimName nvarchar(255),
	@SAMLAttributeName nvarchar(50),
	@ProductId int,
	@Original_ClaimId bigint,
	@Original_ClaimName nvarchar(255),
	@IsNull_SAMLAttributeName Int,
	@Original_SAMLAttributeName nvarchar(50),
	@Original_ProductId int,
	@ClaimId bigint
)
AS
BEGIN
	SET NOCOUNT OFF;
	UPDATE [Auth].[Claim] 
		SET 
			[ClaimName] = @ClaimName, 
			[SAMLAttributeName] = @SAMLAttributeName, 
			[ProductId] = @ProductId 
		WHERE 
			(([ClaimId] = @Original_ClaimId) 
			AND ([ClaimName] = @Original_ClaimName) 
			AND ((@IsNull_SAMLAttributeName = 1 AND [SAMLAttributeName] IS NULL) OR ([SAMLAttributeName] = @Original_SAMLAttributeName)) 
			AND ([ProductId] = @Original_ProductId));
	
	SELECT 
		  ClaimId
		, ClaimName
		, SAMLAttributeName
		, ProductId 
	FROM 
		Auth.Claim 
		WHERE (ClaimId = @ClaimId)
END
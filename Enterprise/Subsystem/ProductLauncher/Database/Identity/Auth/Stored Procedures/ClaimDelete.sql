CREATE PROCEDURE [Auth].[ClaimDelete]
(
	@Original_ClaimId bigint,
	@Original_ClaimName nvarchar(255),
	@IsNull_SAMLAttributeName Int,
	@Original_SAMLAttributeName nvarchar(50),
	@Original_ProductId int
)
AS
BEGIN
	SET NOCOUNT OFF;
	DELETE FROM 
		[Auth].[Claim] 
	WHERE 
		(([ClaimId] = @Original_ClaimId) 
			AND ([ClaimName] = @Original_ClaimName) 
			AND ((@IsNull_SAMLAttributeName = 1 AND [SAMLAttributeName] IS NULL) OR ([SAMLAttributeName] = @Original_SAMLAttributeName)) 
			AND ([ProductId] = @Original_ProductId))

	SELECT @@RowCount [RowsAffected]  
END

CREATE PROCEDURE [Auth].[ClientUserClaimDelete]
(
	@Original_ClientUserClaimId bigint,
	@Original_ClientId int,
	@Original_ClaimId bigint
)
AS
BEGIN
	SET NOCOUNT OFF;
	DELETE FROM [Auth].[ClientUserClaim] 
	WHERE 
		(([ClientUserClaimId] = @Original_ClientUserClaimId) 
		AND ([ClientId] = @Original_ClientId) 
		AND ([ClaimId] = @Original_ClaimId))

	SELECT @@RowCount [RowsAffected]
END

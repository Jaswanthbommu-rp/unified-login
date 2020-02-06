CREATE PROCEDURE [Auth].[ClientUserClaimUpdate]
(
	@ClientId int,
	@ClaimId bigint,
	@Original_ClientUserClaimId bigint,
	@Original_ClientId int,
	@Original_ClaimId bigint,
	@ClientUserClaimId bigint
)
AS
BEGIN
	SET NOCOUNT OFF;
	UPDATE [Auth].[ClientUserClaim] 
	SET 
		[ClientId] = @ClientId
		, [ClaimId] = @ClaimId 
	WHERE 
		(([ClientUserClaimId] = @Original_ClientUserClaimId) 
		AND ([ClientId] = @Original_ClientId) 
		AND ([ClaimId] = @Original_ClaimId));
	
	SELECT 
		  ClientUserClaimId
		, ClientId
		, ClaimId 
	FROM 
		Auth.ClientUserClaim 
	WHERE 
		(ClientUserClaimId = @ClientUserClaimId)
END
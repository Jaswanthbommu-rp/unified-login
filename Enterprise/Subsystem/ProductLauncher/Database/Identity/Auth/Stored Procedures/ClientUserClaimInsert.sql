CREATE PROCEDURE [Auth].[ClientUserClaimInsert]
(
	@ClientId int,
	@ClaimId bigint
)
AS
BEGIN
	SET NOCOUNT OFF;
	INSERT INTO [Auth].[ClientUserClaim] 
		([ClientId], [ClaimId]) 
	VALUES 
		(@ClientId, @ClaimId);
	
	SELECT 
		  ClientUserClaimId
		, ClientId
		, ClaimId 
	FROM 
		Auth.ClientUserClaim 
	WHERE 
		(ClientUserClaimId = SCOPE_IDENTITY())
END

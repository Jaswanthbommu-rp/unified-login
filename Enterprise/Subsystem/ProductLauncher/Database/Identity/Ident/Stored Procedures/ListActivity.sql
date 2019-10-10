CREATE PROCEDURE [Ident].[ListActivity] (
	@PartyId BIGINT
)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT	ia.PartyId,
				ia.ActivityTypeId ,
				ia.[ActivityConfigurationId],
				iat.[ActivityCode],
				iat.[Description],
				ia.[MaxActivityAttemptCount],
				ia.[ActivityTokenExpirationMinutes]
	FROM	[Ident].[ActivityConfiguration] ia
				INNER JOIN Ident.ActivityType iat ON iat.ActivityTypeId = ia.ActivityTypeId
	WHERE	ia.PartyId = @PartyId;
END;
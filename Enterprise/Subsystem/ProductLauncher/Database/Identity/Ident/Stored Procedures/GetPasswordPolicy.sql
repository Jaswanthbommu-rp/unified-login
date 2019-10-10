CREATE PROCEDURE [Ident].[GetPasswordPolicy] (
	@PartyId bigint = NULL
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT	pp.[PasswordPolicyId],
			pp.[PartyId],
			p.[Name],
			pp.[MinimumLength],
			pp.[MaximumLength],
			pp.[MinimumLowercase],
			pp.[MinimumUppercase],
			pp.[MinimumNumeric],
			pp.[MinimumSpecialCharacter],
			pp.[AllowUsersToChangeOwnPassword],
			pp.[EnablePasswordExpiration],
			pp.[PasswordExpirationPeriodInDays],
			pp.[PreventPasswordReuse],
			pp.[NumberOfPasswordsToRemember],
			pp.[UserId],
			pp.[SysStartDateTime],
			pp.[SysEndDateTime]
	FROM	[Ident].[PasswordPolicy] pp WITH (NOLOCK)
			INNER JOIN Enterprise.Organization p WITH (NOLOCK) ON (pp.PartyId = p.PartyId)
	WHERE	pp.[PartyId] = @PartyId OR @PartyId IS NULL
END
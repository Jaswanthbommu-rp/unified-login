IF OBJECT_ID('[Auth].[GetPasswordPolicy]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetPasswordPolicy];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetPasswordPolicy] (
	@PortfolioId int = NULL
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT	pp.[PasswordPolicyId],
			pp.[PortfolioId],
			p.[PortfolioName],
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
	FROM	[Auth].[PasswordPolicy] pp WITH (NOLOCK)
			INNER JOIN [Auth].[Portfolio] p WITH (NOLOCK) ON (pp.PortfolioId = p.PortfolioId)
	WHERE	pp.[PortfolioId] = @PortfolioId
	OR		@PortfolioId IS NULL
END
GO

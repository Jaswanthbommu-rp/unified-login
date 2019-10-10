IF OBJECT_ID('[Ident].[GetUserLoginByName]') IS NOT NULL
	DROP PROCEDURE [Ident].[GetUserLoginByName];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[GetUserLoginByName] (
    @EnterpriseUserName  varchar(255)
)
AS
BEGIN
	SELECT	ul.UserId,
			ul.PartyId,
			ul.[LoginName],
			ul.PasswordModifiedDate,
			p.RealPageId,
			ul.PasswordHash,
			ul.PasswordSalt,
			ul.FromDate,
			ul.ThruDate
	FROM	Ident.UserLogin ul
			JOIN Enterprise.Party p ON p.PartyId = ul.PartyId
	WHERE	ul.[LoginName] = @EnterpriseUserName
END
GO

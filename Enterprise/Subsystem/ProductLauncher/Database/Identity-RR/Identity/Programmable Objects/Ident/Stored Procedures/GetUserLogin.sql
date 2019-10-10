IF OBJECT_ID('[Ident].[GetUserLogin]') IS NOT NULL
	DROP PROCEDURE [Ident].[GetUserLogin];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[GetUserLogin] (
    @RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	SELECT	ul.UserId,
			ul.PartyId,
			ul.[LoginName],
			p.RealPageId,
			ul.PasswordModifiedDate,
			ul.PasswordHash,
			ul.PasswordSalt,
			ul.FromDate,
			ul.ThruDate
	FROM	Ident.UserLogin ul
			JOIN Enterprise.Party p ON p.PartyId = ul.PartyId
	WHERE	p.RealPageId = @RealPageId
END
GO

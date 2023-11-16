CREATE PROCEDURE [Ident].[GetUserLogin]
(
	@RealPageId UNIQUEIDENTIFIER = NULL,
	@UserId     bigINT              = NULL
)
AS
BEGIN
	IF @UserId IS NOT NULL
	BEGIN
		SELECT DISTINCT
			ul.UserId,
			ul.PersonPartyId AS PartyId,
			ul.[LoginName],
			p.RealPageId,
			ULP.StatusTypeId AS StatusId,
			ul.PasswordModifiedDate,
			ul.PasswordHash,
			ul.PasswordSalt,
			ULP.FromDate,
			ULP.ThruDate,
			ULP.StatusThruDate,
			ULP.LastLoginDate [LastLogin],
			'' AS [TimeZoneOffset],--MS.Value [TimeZoneOffset],
			ul.TwoFactorEnabled [TwoFactorEnabled],
			ul.TwoFactorLastNotifyDate [TwoFactorLastNotifyDate]
		FROM Ident.UserLogin ul
			INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = Ul.UserId
			JOIN Enterprise.Party p ON p.PartyId = ul.PersonPartyId
		WHERE
			UL.UserId = @UserId
			AND ULP.PrimaryOrganization = 1 -- ONLY JOIN TO THE PRIMARY ORG FOR THIS PROC  ;
	END
	ELSE
	BEGIN
		SELECT DISTINCT
			ul.UserId,
			ul.PersonPartyId AS PartyId,
			ul.[LoginName],
			p.RealPageId,
			ULP.StatusTypeId AS StatusId,
			ul.PasswordModifiedDate,
			ul.PasswordHash,
			ul.PasswordSalt,
			ULP.FromDate,
			ULP.ThruDate,
			ULP.StatusThruDate,
			ULP.LastLoginDate [LastLogin],
			'' AS [TimeZoneOffset],--MS.Value [TimeZoneOffset],
			ul.TwoFactorEnabled [TwoFactorEnabled],
			ul.TwoFactorLastNotifyDate [TwoFactorLastNotifyDate]
		FROM Ident.UserLogin ul
			INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = Ul.UserId
			JOIN Enterprise.Party p ON p.PartyId = ul.PersonPartyId
		WHERE
			p.RealPageId = @RealPageId
			AND ULP.PrimaryOrganization = 1 -- ONLY JOIN TO THE PRIMARY ORG FOR THIS PROC  ;
	END
END;
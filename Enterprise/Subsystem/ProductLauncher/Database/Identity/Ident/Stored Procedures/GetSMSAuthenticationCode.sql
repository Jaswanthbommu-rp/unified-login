CREATE PROCEDURE [Ident].[GetSMSAuthenticationCode]
(@UserId bigint
)
AS
     BEGIN
        SELECT ac.AuthenticationCode, ac.ExpirationTime
        FROM SMSAuthenticationCode ac
        WHERE(ac.UserId = @UserId)
     END;
GO

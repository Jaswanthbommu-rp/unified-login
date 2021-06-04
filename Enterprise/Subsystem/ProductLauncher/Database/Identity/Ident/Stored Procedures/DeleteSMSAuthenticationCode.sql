CREATE PROCEDURE [Ident].[DeleteSMSAuthenticationCode]
(@UserId bigint
)
AS
     BEGIN
        DELETE 
		FROM [Ident].[SMSAuthenticationCode]
		WHERE UserId = @UserId
     END;
GO

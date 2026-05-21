CREATE PROCEDURE [Ident].[AddSMSAuthenticationCode] (
	@UserId bigint,
	@AuthCode varchar(10)
)
AS
BEGIN
	BEGIN TRY
		INSERT INTO [Ident].[SMSAuthenticationCode](UserId, AuthenticationCode, ExpirationTime)
		VALUES(@UserId, @AuthCode, DATEADD(minute,5,GETUTCDATE()))
	END TRY
	BEGIN CATCH
		DECLARE @ErrorLogID int;
		EXEC dbo.LogError
			@ErrorLogID = @ErrorLogID OUTPUT;
		SELECT	0 AS Id,
					ErrorMessage
		FROM	dbo.ErrorLog
		WHERE	ErrorLogID = @ErrorLogID;
		ROLLBACK;
	END CATCH;
END;
GO

IF OBJECT_ID('[Ident].[UpdateUserLogin]') IS NOT NULL
	DROP PROCEDURE [Ident].[UpdateUserLogin];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[UpdateUserLogin] (
    @RealPageId UNIQUEIDENTIFIER ,
    @LoginName VARCHAR(255) = NULL ,
    @PasswordHash NVARCHAR(255) = NULL ,
    @PasswordSalt NVARCHAR(255) = NULL ,
    @FromDate DATETIME = NULL ,
    @ThruDate DATETIME = NULL
)
AS
BEGIN
	BEGIN TRY
		
		-- BEGIN USER UPDATE --
        BEGIN TRANSACTION;
		UPDATE  UserLogin
		SET     [LoginName] = ISNULL(@LoginName, [LoginName]) ,
				[PasswordHash] = ISNULL(@PasswordHash, PasswordHash) ,
				[PasswordSalt] = ISNULL(@PasswordSalt, PasswordSalt) ,
				[FromDate] = ISNULL(@FromDate, FromDate) ,
				[ThruDate] = CASE WHEN @ThruDate = '12/31/9999' THEN NULL ELSE ISNULL(@ThruDate, ThruDate) END
		OUTPUT	inserted.UserId AS Id,
				'' AS ErrorMessage
		FROM    Ident.UserLogin
				JOIN Enterprise.Party ON Party.PartyId = UserLogin.PartyId
		WHERE   RealPageId = @RealPageId;

		-- END USER UPDATE --

		COMMIT;
	END TRY  
	BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END;
GO

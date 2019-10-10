IF OBJECT_ID('[Ident].[CreateUserLogin]') IS NOT NULL
	DROP PROCEDURE [Ident].[CreateUserLogin];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[CreateUserLogin]
    (
      @RealPageId UNIQUEIDENTIFIER ,
      @LoginName NVARCHAR(255) ,
      @FromDate DATETIME = NULL ,
      @ThruDate DATETIME = NULL
    )
AS
    BEGIN
        BEGIN TRY
            DECLARE @PartyId BIGINT;
            DECLARE @UserId BIGINT;

            SELECT  @PartyId = PartyId
            FROM    Enterprise.Party
            WHERE   RealPageId = @RealPageId;

			IF @FromDate IS NULL
				SELECT  @FromDate = GETUTCDATE();

			BEGIN TRAN
            IF ( SELECT 1
                 FROM   Ident.UserLogin
                 WHERE  [LoginName] = @LoginName
               ) IS NOT NULL
                BEGIN
                    RAISERROR('The User Login already exists', 10, 1);
                END;
            ELSE
                BEGIN
                    IF @PartyId IS NOT NULL
                        BEGIN
                            INSERT  INTO Ident.UserLogin
                                    ( PartyId ,
                                      [LoginName] ,
                                      FromDate ,
                                      ThruDate
						            )
                            VALUES  ( @PartyId , -- PartyId - bigint
                                      @LoginName ,
                                      @FromDate,
                                      @ThruDate
						            );

                            SET @UserId = SCOPE_IDENTITY();							

                            INSERT  INTO Ident.[UserCurrentStatus]
                                    ( UserId ,
                                      StatusTypeId ,
                                      StatusSetDate ,
                                      FromDate 
				                    )
                            VALUES  ( @UserId ,
                                      1 ,
                                      GETUTCDATE() ,
                                      @FromDate
                                    );
                        END;
                END;

            SELECT  @UserId AS Id ,
                    '' AS ErrorMessage;
            COMMIT;
        END TRY
        BEGIN CATCH
            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT  0 AS Id ,
                    ErrorMessage
            FROM    dbo.ErrorLog
            WHERE   ErrorLogID = @ErrorLogID;

            ROLLBACK;
        END CATCH;
    END;
GO

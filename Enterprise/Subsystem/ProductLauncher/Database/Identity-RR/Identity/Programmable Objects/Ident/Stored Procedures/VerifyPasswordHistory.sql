IF OBJECT_ID('[Ident].[VerifyPasswordHistory]') IS NOT NULL
	DROP PROCEDURE [Ident].[VerifyPasswordHistory];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[VerifyPasswordHistory]
    @enterpriseUserName AS NVARCHAR(255) ,
    @NewPasswordHash AS NVARCHAR(255) ,
    @MinPasswordtoRemember AS INT = 5
AS
    BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
        SET NOCOUNT ON;

        SELECT  ISNULL(MAX(CASE WHEN p.ChangedPasswordHash = @NewPasswordHash
                                THEN 1
                                ELSE 0
                           END), 0) AS PasswordExists
        FROM    [ident].UserLogin u
                CROSS APPLY ( SELECT TOP ( @MinPasswordtoRemember )
                                        PH.UserId ,
                                        PH.ChangedPasswordHash ,
                                        PH.ChangedPasswordDateTime
                              FROM      [ident].PasswordHistory PH
                              WHERE     PH.UserId = u.UserId
                              ORDER BY  PH.ChangedPasswordDateTime DESC
                            ) AS p
        WHERE   u.LoginName = @enterpriseUserName;

    END;
GO

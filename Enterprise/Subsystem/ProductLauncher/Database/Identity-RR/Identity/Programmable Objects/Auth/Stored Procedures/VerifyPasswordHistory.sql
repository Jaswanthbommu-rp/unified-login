IF OBJECT_ID('[Auth].[VerifyPasswordHistory]') IS NOT NULL
	DROP PROCEDURE [Auth].[VerifyPasswordHistory];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[VerifyPasswordHistory]
    @enterpriseUserName AS NVARCHAR(50) ,
    @NewPasswordHash AS NVARCHAR(1000) ,
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
        FROM    Auth.Users u
                CROSS APPLY ( SELECT TOP ( @MinPasswordtoRemember )
                                        PH.UserId ,
                                        PH.ChangedPasswordHash ,
                                        PH.ChangedPasswordDateTime
                              FROM      Auth.PasswordHistory PH
                              WHERE     PH.UserId = u.UserId
                              ORDER BY  PH.ChangedPasswordDateTime DESC
                            ) AS p
        WHERE   u.LoginId = @enterpriseUserName;

    END;
GO

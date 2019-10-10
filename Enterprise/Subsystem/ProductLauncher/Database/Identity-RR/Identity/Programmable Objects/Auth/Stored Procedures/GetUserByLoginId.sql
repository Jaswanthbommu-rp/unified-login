IF OBJECT_ID('[Auth].[GetUserByLoginId]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetUserByLoginId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetUserByLoginId] @LoginId NVARCHAR(50)
AS
    BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
        SET NOCOUNT ON;

        SELECT  UserId ,
                LoginId ,
                Firstname ,
                LastName ,
                IsActive ,
                PasswordHash ,
				PasswordSalt ,
                IdentityProvider ,
                Title ,
                Email ,
                Phone ,
                IsLocked,
				LastPasswordModifiedDateTime,
				AccountExpiration
        FROM    Auth.Users
        WHERE   LoginId = @LoginId;

    END;
GO

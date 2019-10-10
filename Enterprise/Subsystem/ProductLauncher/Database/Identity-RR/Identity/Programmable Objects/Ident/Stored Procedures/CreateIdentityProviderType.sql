IF OBJECT_ID('[Ident].[CreateIdentityProviderType]') IS NOT NULL
	DROP PROCEDURE [Ident].[CreateIdentityProviderType];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[CreateIdentityProviderType] (
	@Name nvarchar(50),
	@Description nvarchar(50)
)
AS
BEGIN
	--This is to create a new global type. Currently, we have Azure AD, Okta, and Identity Server
	BEGIN TRY
	    BEGIN TRANSACTION; 

		INSERT INTO [Ident].[IdentityProviderType]
		(
			[Name],
			[Description]
		)
		OUTPUT	Inserted.IdentityProviderTypeId AS Id,
				'' AS ErrorMessage
		VALUES
		(
			@Name,
			@Description
		)

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
    END CATCH;
END
GO

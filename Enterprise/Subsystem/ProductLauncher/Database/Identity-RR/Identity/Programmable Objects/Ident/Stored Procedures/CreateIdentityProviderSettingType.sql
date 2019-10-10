IF OBJECT_ID('[Ident].[CreateIdentityProviderSettingType]') IS NOT NULL
	DROP PROCEDURE [Ident].[CreateIdentityProviderSettingType];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[CreateIdentityProviderSettingType] (
	@IdentityProviderTypeId int,
	@Name nvarchar(50)
)
AS
BEGIN
	--This is to create a new attribute to use when setting up a new Identity Provider.
	BEGIN TRY
	    BEGIN TRANSACTION; 

		INSERT INTO [Ident].[IdentityProviderSettingType]
		(
			[IdentityProviderTypeId],
			[Name]
		)
		OUTPUT	Inserted.IdentityProviderSettingTypeId AS Id,
				'' AS ErrorMessage
		VALUES
		(
			@IdentityProviderTypeId,
			@Name
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

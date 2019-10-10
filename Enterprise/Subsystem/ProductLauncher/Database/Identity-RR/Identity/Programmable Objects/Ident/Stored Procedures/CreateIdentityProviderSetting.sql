IF OBJECT_ID('[Ident].[CreateIdentityProviderSetting]') IS NOT NULL
	DROP PROCEDURE [Ident].[CreateIdentityProviderSetting];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[CreateIdentityProviderSetting] (
	@IdentityProviderSettingTypeId int,
	@Value nvarchar(255)
)
AS
BEGIN
	--This will assign a runtime value to a setting type for a specific identity provider type.
	BEGIN TRY
	    BEGIN TRANSACTION;

		--DECLARE @IdentityProviderTypeId INT,
		--	@IdentityProviderSettingTypeId INT;
	
		--SELECT	@IdentityProviderTypeId = IdentityProviderTypeId
		--FROM	[Ident].IdentityProviderType
		--WHERE	[Name] = @IdentityproviderTypeName;

		--SELECT	@IdentityProviderSettingTypeId = IdentityProviderSettingTypeId
		--FROM	[Ident].IdentityProviderSettingType
		--WHERE	IdentityProviderTypeId = @IdentityProviderTypeId
		--AND		Name = @IdentityProviderSettingTypeName

		INSERT INTO [Ident].[IdentityProviderSetting]
		(
			[IdentityProviderSettingTypeId],
			[Value]
		)
		OUTPUT	Inserted.IdentityProviderSettingId AS Id,
				'' AS ErrorMessage
		VALUES
		(
			@IdentityProviderSettingTypeId,
			@Value
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

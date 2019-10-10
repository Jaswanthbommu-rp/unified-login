IF OBJECT_ID('[Ident].[LinkIdentityProviderSettingToContactMechanism]') IS NOT NULL
	DROP PROCEDURE [Ident].[LinkIdentityProviderSettingToContactMechanism];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[LinkIdentityProviderSettingToContactMechanism] (
	@ContactMechanismId int,
	@IdentityProviderSettingId int,
	@FromDate DateTime,
	@ThruDate DateTime = NULL
)
AS
BEGIN
	--This is the stored procedure that takes a runtime value for an identity provider
	--setting for a given identity provider type and links it to a ContactMechanismId
	BEGIN TRY
	    BEGIN TRANSACTION;

		INSERT INTO [Ident].[ContactMechanismIdentity] (
			[ContactMechanismId],
			[IdentityProviderSettingId],
			[FromDate],
			[ThruDate]
		)
		OUTPUT	Inserted.ContactMechanismIdentityId AS Id,
				'' AS ErrorMessage
		VALUES (
			@ContactMechanismId, -- ContactMechanismId - int
			@IdentityProviderSettingId, -- IdentityProviderSettingId - int
			@FromDate, -- FromDate - datetime
			@ThruDate  -- ThruDate - datetime
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

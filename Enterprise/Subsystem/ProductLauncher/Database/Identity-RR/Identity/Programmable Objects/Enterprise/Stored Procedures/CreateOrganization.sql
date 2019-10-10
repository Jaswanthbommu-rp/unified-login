IF OBJECT_ID('[Enterprise].[CreateOrganization]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[CreateOrganization];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[CreateOrganization]
    @OrganizationId UNIQUEIDENTIFIER = NULL,
	@OrganizationName NVARCHAR(50)
AS
BEGIN
	BEGIN TRY
		IF @OrganizationId IS NULL
		BEGIN
		    SET @OrganizationId = NEWID();
		END

		BEGIN TRANSACTION
		SET NOCOUNT ON
		DECLARE @PartyId BIGINT;

		INSERT  INTO [Enterprise].Party
		(
			RealPageId,
			CreateDate
		)
		VALUES
		(
			@OrganizationId,
			GETUTCDATE()
		);

		SET @PartyId = SCOPE_IDENTITY();

		INSERT INTO [Enterprise].Organization
		(
			PartyId,
			Name
		)
		VALUES
		(
			@PartyId,
			@OrganizationName
		)
		SET NOCOUNT OFF
		COMMIT;
		SELECT	@PartyId AS Id,
				@OrganizationId AS RealPageId,
				'' AS ErrorMessage
	END TRY
	BEGIN CATCH
		ROLLBACK;

		DECLARE @ErrorLogID INT;
		EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

		SELECT  0 AS Id,
				ErrorMessage
		FROM    [dbo].ErrorLog
		WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END
GO

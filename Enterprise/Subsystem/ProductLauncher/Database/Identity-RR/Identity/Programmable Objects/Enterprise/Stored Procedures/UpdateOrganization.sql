IF OBJECT_ID('[Enterprise].[UpdateOrganization]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[UpdateOrganization];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[UpdateOrganization]
    @OrganizationId UNIQUEIDENTIFIER,
	@OrganizationName NVARCHAR(50)
AS
    BEGIN
		BEGIN TRY
			SET NOCOUNT ON;
			BEGIN TRANSACTION;
			UPDATE o
			SET Name = @OrganizationName
			FROM [Enterprise].Organization o
			JOIN [Enterprise].Party p ON p.PartyId = o.PartyId
			WHERE p.RealPageId = @OrganizationId
			SET NOCOUNT OFF;

			COMMIT;
			SELECT @OrganizationId AS RealPageId
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
    END;
GO

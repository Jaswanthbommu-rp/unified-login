IF OBJECT_ID('[Person].[LinkGeographicBoundaryToContactMechanism]') IS NOT NULL
	DROP PROCEDURE [Person].[LinkGeographicBoundaryToContactMechanism];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Person].[LinkGeographicBoundaryToContactMechanism] (
	@ContactMechanismId INT,
	@GeographicBoundaryId INT,
	@FromDate DATETIME,
	@ThruDate DATETIME = NULL
)
AS
BEGIN
	BEGIN TRY
	    BEGIN TRANSACTION;

		INSERT INTO Enterprise.ContactMechanismBoundary (
			ContactMechanismId,
			GeographicBoundaryId,
			FromDate,
			ThruDate
		)
		OUTPUT	Inserted.ContactMechanismBoundaryId AS Id,
				'' AS ErrorMessage
		VALUES (
			@ContactMechanismId, -- ContactMechanismId - int
			@GeographicBoundaryId, -- GeographicBoundaryId - int
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

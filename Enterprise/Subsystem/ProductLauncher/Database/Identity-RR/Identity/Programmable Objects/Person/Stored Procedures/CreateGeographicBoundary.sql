IF OBJECT_ID('[Person].[CreateGeographicBoundary]') IS NOT NULL
	DROP PROCEDURE [Person].[CreateGeographicBoundary];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Person].[CreateGeographicBoundary] (
    @TypeName NVARCHAR(50),
    @Value NVARCHAR(50),
    @Code NVARCHAR(50),
    @Abbreviation NVARCHAR(10)
)
AS
BEGIN
	BEGIN TRY
	    BEGIN TRANSACTION; 

		DECLARE @GeographicBoundaryId INT,
			@GeographicBoundaryTypeId INT;
	
		SELECT	@GeographicBoundaryTypeId = GeographicBoundaryTypeId
		FROM	Enterprise.GeographicBoundaryType
		WHERE	[Name] = @TypeName;
	
		-- Check if the boundary already exists.
		-- Return the Boundary Id if it already exists.

		SELECT	@GeographicBoundaryId = GeographicBoundaryId
		FROM	Enterprise.GeographicBoundary
		WHERE	GeographicBoundaryTypeId = @GeographicBoundaryTypeId
		AND		[Name] = @Value;

		IF @GeographicBoundaryId IS NOT NULL
		BEGIN
			SELECT	@GeographicBoundaryId AS Id,
					'' AS ErrorMessage;
		END;
		ELSE
		BEGIN
			INSERT  INTO Enterprise.GeographicBoundary
			(
				GeographicBoundaryTypeId,
				Name,
				GeographicBoundaryCode,
				Abbreviation
			)
			OUTPUT	Inserted.GeographicBoundaryId AS Id,
					'' AS ErrorMessage
			VALUES
			(
				@GeographicBoundaryTypeId,
				@Value,
				@Code,
				@Abbreviation
			);
		END;

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				'' AS RealPageId,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO

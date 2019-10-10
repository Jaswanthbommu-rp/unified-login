IF OBJECT_ID('[Enterprise].[LinkProductSettingToConfiguration]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[LinkProductSettingToConfiguration];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[LinkProductSettingToConfiguration] (
	@ConfigurationId INT,
	@ProductSettingId INT,
	@FromDate DATETIME = NULL,
	@ThruDate DATETIME = NULL
)
AS
BEGIN
	DECLARE @UTCDATE datetime = GETUTCDATE();

	BEGIN TRY
        BEGIN TRANSACTION;
		UPDATE	Enterprise.ProductConfiguration
		SET		ThruDate = @UTCDATE
		WHERE	ConfigurationId = @ConfigurationId
		AND		ProductSettingId = @ProductSettingId
		AND		(ThruDate >= @UTCDATE OR ThruDate IS NULL)

		INSERT INTO Enterprise.ProductConfiguration (
			ConfigurationId,
			ProductSettingId,
			FromDate,
			ThruDate
		)
		OUTPUT	Inserted.ProductConfigurationId AS Id,
				'' AS ErrorMessage
		VALUES (
			@ConfigurationId,
			@ProductSettingId,
			ISNULL(@FromDate, @UTCDATE),
			@ThruDate
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
	END CATCH
END;
GO

CREATE PROCEDURE Enterprise.LinkProductSettingToConfiguration (
	@ConfigurationId INT,
	@ProductSettingId INT,
	@FromDate DATETIME = NULL,
	@ThruDate DATETIME = NULL
)
AS
BEGIN
	DECLARE @UTCDATE datetime = dateadd(MINUTE, -1, GETUTCDATE());

	BEGIN TRY
        BEGIN TRANSACTION;

		DECLARE @ProductSettingTypeId INT
		SELECT @ProductSettingTypeId = ProductSettingTypeId from Enterprise.ProductSetting WHERE ProductSettingId = @ProductSettingId

		IF EXISTS (SELECT TOP 1 1 from Enterprise.ProductConfiguration PC 
					INNER JOIN Enterprise.ProductSetting PS ON PC.ProductSettingId = PS.ProductSettingId
				   WHERE
						PC.ConfigurationId = @ConfigurationId
						AND
						PS.ProductSettingTypeId = @ProductSettingTypeId
						AND
						PC.ThruDate IS NULL
		)
		BEGIN
			UPDATE PC
			SET		ThruDate = @UTCDATE
			FROM 
			Enterprise.ProductConfiguration PC 
					INNER JOIN Enterprise.ProductSetting PS ON PC.ProductSettingId = PS.ProductSettingId
				   WHERE
						PC.ConfigurationId = @ConfigurationId
						AND
						PS.ProductSettingTypeId = @ProductSettingTypeId
						AND 
						(PC.ThruDate >= @UTCDATE OR PC.ThruDate IS NULL)
		END
		
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
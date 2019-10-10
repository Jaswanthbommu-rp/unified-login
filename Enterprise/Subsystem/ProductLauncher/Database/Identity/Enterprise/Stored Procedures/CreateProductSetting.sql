CREATE PROCEDURE [Enterprise].[CreateProductSetting] (
	@ProductId INT,
	@ProductSettingTypeId INT,
	@Value NVARCHAR(1000),
	@FromDate DATETIME,
	@ThruDate DATETIME = NULL,
	@ProductSettingId INT OUTPUT
)
AS
BEGIN
	SET @ProductSettingId = NULL
	BEGIN TRY
		SELECT	@ProductSettingId = ps.ProductSettingId
		FROM	Enterprise.ProductSetting ps
				INNER JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
		WHERE	ps.ProductId = @ProductId
		AND		ps.Value = @Value
		AND		ps.ProductSettingTypeId = @ProductSettingTypeId
		AND		(ps.ThruDate >= @ThruDate OR ThruDate IS NULL);

		IF @ProductSettingId IS NULL
		BEGIN
			INSERT INTO Enterprise.ProductSetting (
				ProductId,
				ProductSettingTypeId,
				Value,
				FromDate,
				ThruDate
			)
			OUTPUT	Inserted.ProductSettingId AS Id,
					'' AS ErrorMessage
			VALUES (
				@ProductId,
				@ProductSettingTypeId,
				@Value,
				ISNULL(@FromDate, GETUTCDATE()),
				@ThruDate
			)

			SET @ProductSettingId = SCOPE_IDENTITY();
		END
		ELSE
		BEGIN
			SELECT @ProductSettingId as Id, '' AS ErrorMessage
		END
	END TRY  
	BEGIN CATCH
        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END;
CREATE PROCEDURE [Enterprise].[UpdatePersonaConfiguration]
	@PersonaId BIGINT,
	@ProductId INT,
	@ProductSettingID INT
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @SettingValue NVARCHAR(2)
		
	SELECT
		@SettingValue = Value
	FROM
		Enterprise.ProductSetting 
	WHERE
		ProductSettingId = @ProductSettingID

	BEGIN TRY
			
		IF EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSetting PS INNER JOIN Enterprise.ProductSettingType PST on PS.ProductSettingTypeId = PST.ProductSettingTypeId
			WHERE PS.ProductSettingId = @ProductSettingID AND PST.Name = 'ProductStatus' )
		BEGIN
			UPDATE 
				Enterprise.PersonaConfiguration
				SET StatusTypeId = @SettingValue
				,ProductDeactivationDate = CASE WHEN (@SettingValue = 19 OR @SettingValue = 10) THEN GETUTCDATE() ELSE NULL END
			WHERE
				PersonaId = @PersonaId
				AND
				ProductId = @ProductId
				AND
				ThruDate IS NULL
			
			SELECT	@PersonaId AS Id ,
                '' AS ErrorMessage
		END	

		IF EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSetting PS INNER JOIN Enterprise.ProductSettingType PST on PS.ProductSettingTypeId = PST.ProductSettingTypeId
			WHERE PS.ProductSettingId = @ProductSettingID AND PST.Name = 'IsFavorite' )
		BEGIN
			UPDATE 
				Enterprise.PersonaConfiguration
				SET IsFavorite = @SettingValue
			WHERE
				PersonaId = @PersonaId
				AND
				ProductId = @ProductId
				AND
				ThruDate IS NULL
			
			SELECT	@PersonaId AS Id ,
                '' AS ErrorMessage
		END

		IF EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSetting PS INNER JOIN Enterprise.ProductSettingType PST on PS.ProductSettingTypeId = PST.ProductSettingTypeId
			WHERE PS.ProductSettingId = @ProductSettingID AND PST.Name = 'UsePrimaryProperties' )
		BEGIN
			UPDATE 
				Enterprise.PersonaConfiguration
				SET UsePrimaryProperties = @SettingValue
			WHERE
				PersonaId = @PersonaId
				AND
				ProductId = @ProductId
				AND
				ThruDate IS NULL
			
			SELECT	@PersonaId AS Id ,
                '' AS ErrorMessage
		END

	END TRY
	BEGIN CATCH

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT	0 AS Id ,
                ErrorMessage
        FROM	dbo.ErrorLog
        WHERE	ErrorLogID = @ErrorLogID;
	END CATCH
END

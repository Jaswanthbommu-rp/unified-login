CREATE PROCEDURE [Enterprise].[UpdateOrganizationProduct] (
	 @PartyId BIGINT
	,@ProductId INT
	,@UsePrimaryProperties tinyint = 0
)
AS
BEGIN
	BEGIN TRY

	DECLARE @UsePrimaryPropertiesTypeId INT
	DECLARE @UPPConfigurationId INT
	DECLARE @UPPProductSettingId INT

	SELECT @UsePrimaryPropertiesTypeId = ProductSettingTypeId FROM Enterprise.ProductSettingType 
	WHERE Name = 'UsePrimaryProperties'

	SELECT @UPPConfigurationId = PC.ConfigurationId, @UPPProductSettingId=PC.ProductSettingId
	FROM Enterprise.ProductSetting PS 
	INNER JOIN Enterprise.ProductConfiguration PC on PS.ProductSettingId = PC.ProductSettingId
	INNER JOIN Enterprise.OrganizationProduct OP on OP.ConfigurationId = PC.ConfigurationId 
		AND OP.PartyId = @PartyId
	INNER JOIN Enterprise.ProductSettingType PST on PS.ProductSettingTypeId = PST.ProductSettingTypeId 
	AND PST.Name = 'UsePrimaryProperties'

	
		IF EXISTS (
			SELECT TOP 1 1 FROM Enterprise.ProductSetting PS 
			WHERE PS.ProductSettingId = @UPPProductSettingId
			AND PS.ProductSettingTypeId = @UsePrimaryPropertiesTypeId
			AND PS.ProductId = @ProductId
			AND ThruDate IS NULL
		)
		BEGIN
		
			UPDATE PS 
			SET Value = @UsePrimaryProperties
				FROM Enterprise.ProductSetting PS 
			WHERE PS.ProductSettingId = @UPPProductSettingId
			AND PS.ProductSettingTypeId = @UsePrimaryPropertiesTypeId
			AND PS.ProductId = @ProductId
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
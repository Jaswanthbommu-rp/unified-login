CREATE PROCEDURE [Enterprise].[CreateOrganizationProduct] (
	 @PartyId BIGINT
	,@ConfigurationID INT = NULL
	,@ProductId INT
	,@UsePrimaryProperties tinyint = 0
	,@FromDate DATETIME = NULL
	,@ThruDate DATETIME = NULL)
AS
BEGIN
	
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

	IF @FromDate IS NULL
		SET @FromDate = GETUTCDATE();

	IF @ConfigurationID IS NULL
		SELECT @ConfigurationID = ConfigurationID FROM GlobalProductConfiguration WHERE ProductId = @ProductId AND ((GETUTCDATE() BETWEEN FromDate AND ThruDate) OR (ThruDate IS NULL));

	
	BEGIN TRY
		INSERT INTO Enterprise.OrganizationProduct ([PartyId],[ConfigurationId],[ProductId],[FromDate],[ThruDate])
		VALUES (@PartyId,@ConfigurationID,@ProductId,@UsePrimaryProperties,@FromDate,@ThruDate);

		SELECT	SCOPE_IDENTITY() AS Id, '' AS ErrorMessage;
		
		--Link Use Primary Properties setting to org product
		DECLARE @ProductSettingIDTable TABLE (ID INT)

		INSERT INTO Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate )
				OUTPUT inserted.ProductSettingId into @ProductSettingIDTable
		VALUES ( @ProductId, @UsePrimaryPropertiesTypeId, @UsePrimaryProperties, GETUTCDATE() )

		INSERT INTO Enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate ) 
		SELECT @ConfigurationId, ID, GETUTCDATE() FROM @ProductSettingIDTable

		IF @ProductId = 26
		BEGIN
			
				DECLARE @UserId bigint,@UARoleId Int
				SELECT	@UserId = UserId FROM	Ident.UserLogin WHERE	LoginName LIKE 'realpagead@%'
				Select @UARoleId = RoleId from [Security].Role where ShortName = 'View.Amenities'

				INSERT INTO Security.OrganizationDefaultRole(OrgPartyId,RoleId,CreatedBy,CreatedDate)
				SELECT @PartyId,@UARoleId,@UserId,GETDATE()
			
					
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
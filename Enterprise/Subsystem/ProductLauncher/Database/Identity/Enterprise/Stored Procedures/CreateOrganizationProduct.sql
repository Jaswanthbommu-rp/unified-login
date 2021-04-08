CREATE PROCEDURE [Enterprise].[CreateOrganizationProduct] (
	 @PartyId BIGINT
	,@ConfigurationID INT = NULL
	,@ProductId INT
	,@FromDate DATETIME = NULL
	,@ThruDate DATETIME = NULL)
AS
BEGIN
	DECLARE @SchemaName varchar(25);
	SELECT	@SchemaName = ps.Value				
	FROM	Enterprise.GlobalProductConfiguration gpc
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
	WHERE  gpc.ProductId = 3
	AND (gpc.ThruDate IS NULL)
	AND ( pc.ThruDate IS NULL)
	AND ( ps.ThruDate IS NULL)
	And PST.Name = 'RolesRightsSchemaName'

	IF @FromDate IS NULL
		SET @FromDate = GETUTCDATE();

	IF @ConfigurationID IS NULL
		SELECT @ConfigurationID = ConfigurationID FROM GlobalProductConfiguration WHERE ProductId = @ProductId AND ((GETUTCDATE() BETWEEN FromDate AND ThruDate) OR (ThruDate IS NULL));

	IF EXISTS(SELECT 1 FROM Enterprise.OrganizationProduct WHERE PartyId = @PartyId AND ProductId = @ProductId)
		UPDATE Enterprise.OrganizationProduct SET ThruDate = GETUTCDATE() WHERE PartyId = @PartyId AND ProductId = @ProductId;
	
	BEGIN TRY
		INSERT INTO Enterprise.OrganizationProduct ([PartyId],[ConfigurationId],[ProductId],[FromDate],[ThruDate])
		VALUES (@PartyId,@ConfigurationID,@ProductId,@FromDate,@ThruDate);

		SELECT	SCOPE_IDENTITY() AS Id, '' AS ErrorMessage;

		IF @ProductId = 26
		BEGIN
			IF (@SchemaName = 'Security')
			BEGIN
				DECLARE @UserId bigint,@UARoleId Int
				SELECT	@UserId = UserId FROM	Ident.UserLogin WHERE	LoginName LIKE 'realpagead@%'
				Select @UARoleId = RoleId from [Security].Role where ShortName = 'View.Amenities'

				INSERT INTO Security.OrganizationDefaultRole(OrgPartyId,RoleId,CreatedBy,CreatedDate)
				SELECT @PartyId,@UARoleId,@UserId,GETDATE()
			END
			ELSE
			BEGIN
				EXECUTE Enterprise.SetupUnifiedAmenities @PartyId;
			END			
		END

		IF @ProductId = 65
		BEGIN
			IF (@SchemaName = 'Security')  
			BEGIN 
				EXECUTE [Enterprise].[SetupSGTRoles] @PartyId;
			END
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
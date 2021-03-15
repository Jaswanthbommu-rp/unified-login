CREATE PROCEDURE [Enterprise].[CreateOrganizationProduct] (
	 @PartyId BIGINT
	,@ConfigurationID INT = NULL
	,@ProductId INT
	,@FromDate DATETIME = NULL
	,@ThruDate DATETIME = NULL)
AS
BEGIN
	DECLARE @SchemaName varchar(25);
	DECLARE @UserId BIGINT
	SELECT @UserId = UserId FROM Ident.UserLogin WHERE LoginName like 'realpagead@%'
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


		-- Adding Default Product Roles for Home Sharing		
		IF (@ProductId = 60 AND EXISTS(SELECT 1 FROM Enterprise.OrganizationProduct WHERE PartyId = @PartyId AND ProductId = @ProductId AND ThruDate IS NULL))
		  BEGIN
			DECLARE @PropertyAdminRoleId INT
			DECLARE @PropertyUserRoleId INT							

			INSERT INTO Security.Role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
			VALUES('Property Admin','Property Admin','Property Admin',3,@PartyId,@ProductId,@UserId,GETDATE())
	
			SELECT @PropertyAdminRoleId = SCOPE_IDENTITY()

			INSERT INTO Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
			VALUES(
			@PropertyAdminRoleId,
			(SELECT RightId FROM Security.[Right] WHERE RightName='PropertyAdmin' AND ProductId = 60),
			@UserId,
			GETDATE())
	
			INSERT INTO Security.Role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
			VALUES('Property User','Property User','Property User',3,@PartyId,@ProductId,@UserId,GETDATE())
	
			SELECT @PropertyUserRoleId = SCOPE_IDENTITY()

			INSERT INTO Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
			VALUES(
			@PropertyUserRoleId,
			(SELECT RightId FROM Security.[Right] WHERE RightName='PropertyUser' AND ProductId = 60),
			@UserId,
			GETDATE())
		  END

		IF @ProductId = 26
		BEGIN
			IF (@SchemaName = 'Security')
			BEGIN
				DECLARE @UARoleId Int				
				Select @UARoleId = RoleId from [Security].Role where ShortName = 'View.Amenities'

				INSERT INTO Security.OrganizationDefaultRole(OrgPartyId,RoleId,CreatedBy,CreatedDate)
				SELECT @PartyId,@UARoleId,@UserId,GETDATE()
			END
			ELSE
			BEGIN
				EXECUTE Enterprise.SetupUnifiedAmenities @PartyId;
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
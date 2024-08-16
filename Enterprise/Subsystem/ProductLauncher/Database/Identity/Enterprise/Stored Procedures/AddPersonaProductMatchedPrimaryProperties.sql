CREATE PROCEDURE [Enterprise].[AddPersonaProductMatchedPrimaryProperties] (
    @ProductId int,
	@PersonaId bigint,
	@PropertyInstanceJSON nvarchar(max),
    @ModifiedBy bigint
 )
AS
BEGIN
    Declare @PartyId bigint
	Declare @OrgSettingValue varchar(2)
	DECLARE @SyncUserProductPrimaryPropertiesForPlatformProduct varchar(2);
	DECLARE @CreateDate datetime2 = GETUTCDATE();
	DECLARE @ModifiedDate datetime2 = GETUTCDATE();
	BEGIN TRY  
		SELECT @PartyId = PA.PartyId      
		FROM  Person.Persona P
		JOIN Ident.UserLoginPersona ULP ON
			ULP.UserLoginPersonaId = P.UserLoginPersonaId
		JOIN Enterprise.Party PA ON
			PA.PartyId = ULP.OrganizationPartyId
		WHERE P.PersonaId = @PersonaId  

		SELECT  @OrgSettingValue = ISNULL(MappingValue,0)  
		FROM [Settings].[OrganizationSettings] 
        WHERE  MappingName = 'PrimaryPropertyEnterpriseRole'
		AND PartyId = @PartyId

		SELECT	@SyncUserProductPrimaryPropertiesForPlatformProduct = ISNULL(ps.Value,'0')				
		FROM	Enterprise.GlobalProductConfiguration gpc
				JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
				JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
				JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
		WHERE  gpc.ProductId = @ProductId
		AND (gpc.ThruDate IS NULL)
		AND ( pc.ThruDate IS NULL)
		AND ( ps.ThruDate IS NULL)
		And PST.Name = 'SyncUserProductPrimaryPropertiesForPlatformProduct'		
		
	    DELETE FROM [Enterprise].[UserSyncProductPrimaryPropertiesStaging] WHERE ProductId = @productId And PersonaId = @personaId

		INSERT INTO [Enterprise].[UserSyncProductPrimaryPropertiesStaging] (
			 [ProductId]
			,[PersonaId]
			,[PropertyInstanceId]
			,[ProductPropertyId]
			,[ProductPropertyName]
			,[ModifiedBy]
			,[ModifiedDate]
			,[CreateDate]
		)

		SELECT @ProductId,@PersonaId, pa.PropertyInstanceId, JSON1.productPropertyId,JSON1.propertyName, @ModifiedBy,@ModifiedDate, @CreateDate
		FROM Enterprise.PropertyInstance pa
				INNER JOIN 
			(SELECT productPropertyId, propertyInstanceId,propertyName  FROM
				OPENJSON (@PropertyInstanceJSON)  
				WITH (
				           productPropertyId varchar(100) '$.productPropertyId',
				           propertyInstanceId varchar(max) '$.propertyInstanceId',
						   propertyName varchar(max) '$.propertyName'
				 )
			 )
			 AS JSON1 ON PA.InstanceId = JSON1.propertyInstanceId 

		-- Sync Primary properties automatically after translation if company turned off use primary properties and product  usersync setting turned on
		IF (@SyncUserProductPrimaryPropertiesForPlatformProduct = '1' AND  @OrgSettingValue = '0')
		BEGIN
			INSERT INTO Enterprise.PropertyInstanceMapping (
						PersonaId,
						ProductId,
						PropertyInstanceId
			)
			SELECT @personaId,
				   @ProductId,
				   PropertyInstanceId
			FROM [Enterprise].[UserSyncProductPrimaryPropertiesStaging]
			WHERE PersonaId = @personaId
			AND PropertyInstanceId <> 0
			AND ProductId = @ProductId
			EXCEPT
			SELECT PersonaId,ProductId,PropertyInstanceId
			FROM	Enterprise.PropertyInstanceMapping
			WHERE PersonaId = @PersonaId
			AND ProductId = @ProductId
			AND ThruDate IS NULL

			IF NOT EXISTS(Select 1 FROM Enterprise.PersonaProductPropertiesSyncHistory WHERE PersonaId = @personaId AND ProductId = @ProductId)
			BEGIN
				INSERT INTO Enterprise.PersonaProductPropertiesSyncHistory(PersonaId,ProductId,ProductPropertiesSyncDate)
				SELECT @PersonaId,@ProductId, GETUTCDATE()
			END
			ELSE
			BEGIN
				UPDATE Enterprise.PersonaProductPropertiesSyncHistory SET ProductPropertiesSyncDate =  GETUTCDATE()
				WHERE PersonaId = @personaId
				AND ProductId = @ProductId
			END	
			
			INSERT INTO Enterprise.PropertyInstanceMapping (
						PersonaId,
						ProductId,
						PropertyInstanceId
				)
			SELECT @personaId,
					3,
					PropertyInstanceId
			FROM Enterprise.PropertyInstanceMapping
			WHERE PersonaId = @PersonaId
			AND ProductId = @ProductId
			AND PropertyInstanceId <> 0
			AND ThruDate IS NULL
			AND NOT EXISTS (SELECT 1 FROM Enterprise.PropertyInstanceMapping WHERE ProductId = 3 AND PropertyInstanceId = -1 AND PersonaId = @PersonaId AND ThruDate IS NULL)
			AND PropertyInstanceId NOT IN (SELECT Distinct PropertyInstanceId	FROM	Enterprise.PropertyInstanceMapping
				WHERE PersonaId = @PersonaId
				AND ProductId = 3
				AND ThruDate IS NULL)		
		END

		SELECT	1 AS Id
	END TRY
	BEGIN CATCH
		DECLARE @ErrorLogID int;
		EXEC dbo.LogError
			@ErrorLogID = @ErrorLogID OUTPUT;
		SELECT	0 AS Id,
						ErrorMessage
		FROM		dbo.ErrorLog
		WHERE	ErrorLogID = @ErrorLogID;
	END CATCH;
END
CREATE PROCEDURE [Enterprise].[AddUpdatePersonaProductPropertyInstanceMapping] (
@Personas	[Enterprise].[SyncPersonaList] READONLY,
@ProductId int,
@RealPageId uniqueidentifier )
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @Now datetime = GETUTCDATE()
	declare @MAX_ID INT
	declare @Current_ID INT = 1
	Declare @UPFMProductId int = 3
	Declare @PartyId bigint
	Declare @OrgSettingValue varchar(2)
	DECLARE @SyncUserProductPrimaryPropertiesForPlatformProduct varchar(2);
	BEGIN TRY
		
		DECLARE @personaList TABLE (
			Id int identity,
			PersonaId bigint
		)

		Insert Into @personaList(PersonaId)
		Select PersonaId From @Personas
		
		SELECT @PartyId = PartyId      
		FROM  Enterprise.Party      
		WHERE RealPageId = @RealPageId  

		SELECT @OrgSettingValue = ISNULL(MS.Value,'0')            
			FROM Enterprise.MasterConfigurationSetting mcs
			INNER JOIN Enterprise.MasterConfiguration mc ON mc.MasterConfigurationId = mcs.MasterConfigurationId
			INNER JOIN Enterprise.MasterSetting ms ON mcs.MasterSettingId = ms.MasterSettingId
			INNER JOIN Enterprise.MasterSettingType mst ON mst.MasterSettingTypeId = ms.MasterSettingTypeId
			INNER JOIN Enterprise.MasterConfigurationType mct ON mct.MasterConfigurationTypeId = mst.MasterConfigurationTypeId
		WHERE MST.Name = 'EnablePrimaryPropertiesAndEnterpriseRoles'
		AND MCT.Name = 'Organization'
		AND  MC.AttributeId = @PartyId;

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

		SELECT @MAX_ID = max(Id) from @personaList

		WHILE @Current_ID <= @MAX_ID
		BEGIN
			Declare @personaId bigint
			SELECT @personaId = PersonaId
			FROM @personaList WHERE Id = @Current_ID

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
			AND ProductId = @ProductId
			EXCEPT
			SELECT PersonaId,ProductId,PropertyInstanceId
			FROM	Enterprise.PropertyInstanceMapping
			WHERE PersonaId = @personaId
			AND ProductId = @ProductId
			AND ThruDate IS NULL

			IF NOT EXISTS(Select 1 FROM Enterprise.PersonaProductPropertiesSyncHistory WHERE PersonaId = @personaId AND ProductId = @ProductId)
			BEGIN
				INSERT INTO Enterprise.PersonaProductPropertiesSyncHistory(PersonaId,ProductId,ProductPropertiesSyncDate)
				SELECT @personaId,@ProductId,@Now
			END
			ELSE
			BEGIN
				UPDATE Enterprise.PersonaProductPropertiesSyncHistory SET ProductPropertiesSyncDate = @Now
				WHERE PersonaId = @personaId
				AND ProductId = @ProductId
			END			

			IF ((@SyncUserProductPrimaryPropertiesForPlatformProduct = '1' AND  @OrgSettingValue = '0') OR (@OrgSettingValue = '1'))
			BEGIN
				INSERT INTO Enterprise.PropertyInstanceMapping (
						PersonaId,
						ProductId,
						PropertyInstanceId
				)
				SELECT @personaId,
					   @UPFMProductId,
					   PropertyInstanceId
				FROM Enterprise.PropertyInstanceMapping
				WHERE PersonaId = @personaId
				AND ProductId = @ProductId
				AND ThruDate IS NULL
				AND PropertyInstanceId NOT IN (SELECT Distinct PropertyInstanceId	FROM	Enterprise.PropertyInstanceMapping
					WHERE PersonaId = @personaId
					AND ProductId = @UPFMProductId
					AND ThruDate IS NULL)				
			END

			Set @Current_ID = @Current_ID + 1
		END

		SELECT	1 AS Id,'' AS ErrorMessage;
	END TRY
	BEGIN CATCH
		DECLARE @ErrorLogID INT;
		EXEC dbo.LogError
			@ErrorLogID = @ErrorLogID OUTPUT;
	
		SELECT	0 AS Id,
						ErrorMessage
		FROM		dbo.ErrorLog
		WHERE	ErrorLogID = @ErrorLogID;
	END CATCH;
END

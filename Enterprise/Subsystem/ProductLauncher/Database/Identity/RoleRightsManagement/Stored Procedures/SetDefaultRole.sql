Create PROCEDURE [Security].[SetDefaultRole](@RoleId INT,@PartyId bigint,@CreatedBy nvarchar(50))
AS
     BEGIN
         DECLARE @DefaultRoleId INT;
         DECLARE @SaveEnterpriseRoleData varchar(10);
		 Declare @EnterpriseRolename    NVARCHAR(50),@EnterpriseRoleId INT
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

		SELECT	@SaveEnterpriseRoleData = ps.Value				
		FROM	Enterprise.GlobalProductConfiguration gpc
				JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
				JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
				JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
		WHERE  gpc.ProductId = 3
		AND (gpc.ThruDate IS NULL)
		AND ( pc.ThruDate IS NULL)
		AND ( ps.ThruDate IS NULL)
		And PST.Name = 'SaveRoleDataInEnterprise'
         
		Select @EnterpriseRolename = RoleName From  [Security].[Role] Where RoleId = @RoleId

		IF @PartyId IS NOT NULL
		BEGIN
			BEGIN TRY
				IF EXISTS (SELECT 1 FROM Security.OrganizationDefaultRole Where OrgPartyId = @PartyId)
				BEGIN
					UPDATE Security.OrganizationDefaultRole SET RoleId = @RoleId,
																CreatedBy = @CreatedBy,
																CreatedDate = GETDATE()
					Where OrgPartyId = @PartyId
				END
				ELSE
				BEGIN
					INSERT INTO Security.OrganizationDefaultRole(OrgPartyId,RoleId,CreatedBy,CreatedDate)
					SELECT @PartyId,@RoleId,@CreatedBy,GETDATE()
				END
				 
				SELECT @RoleId AS RoleId,
					'' AS ErrorMessage;

				IF (@SaveEnterpriseRoleData = '1' AND @SchemaName = 'Security')
				BEGIN					
					Select @EnterpriseRoleId = R.RoleID from Enterprise.Role R 
					Join Enterprise.RoleValueType RV ON
						RV.RoleValueTypeId = R.RoleValueTypeId
						AND R.PartyID = @PartyId
					Where RV.Value = @EnterpriseRolename
					
					EXEC [Enterprise].[SetDefaultRole] @EnterpriseRoleId,@CreatedBy
				END
			END TRY
			BEGIN CATCH
				DECLARE @ErrorLogID INT;
				EXEC dbo.LogError
					@ErrorLogID = @ErrorLogID OUTPUT;
				SELECT @RoleId AS Id,
					ErrorMessage
				FROM [dbo].ErrorLog
				WHERE ErrorLogID = @ErrorLogID;
			END CATCH;			
		END
     END;

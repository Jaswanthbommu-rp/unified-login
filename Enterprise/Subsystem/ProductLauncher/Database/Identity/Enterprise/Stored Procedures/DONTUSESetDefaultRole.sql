CREATE PROCEDURE [Enterprise].[SetDefaultRole](@RoleId INT, @CreatedBy nvarchar(50) NULL)
AS
     BEGIN
         DECLARE @DefaultRoleId INT;
         DECLARE @PartyId INT;
		 DECLARE @SchemaName varchar(25);
		 DECLARE @Rolename varchar(256)
		 DECLARE @SecurityRoleId INT

		Select @Rolename = RV.Value, @PartyId = R.PartyID 
		From Enterprise.Role R
		Join Enterprise.RoleValueType RV ON
			RV.RoleValueTypeId = R.RoleValueTypeId
		Where RoleId = @RoleId

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
		                 
		IF @PartyId IS NOT NULL
		BEGIN TRY
			UPDATE Enterprise.Role
			SET
				DefaultRole = 0
			FROM Enterprise.Role R
			INNER JOIN Enterprise.RoleValueType RR ON RR.RoleValueTypeId = R.RoleValueTYpeId
			WHERE R.PartyId = @PartyId AND R.DefaultRole = 1;
             
			UPDATE Enterprise.Role
			SET
				DefaultRole = 1
			WHERE ROleId = @RoleId;
			SELECT @RoleId AS RoleId,
				'' AS ErrorMessage;
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
		--set default in security schema 
		IF (@SchemaName = 'Enterprise')
		BEGIN
			
			Select @SecurityRoleId = R.RoleID from Security.Role R 							
			Where R.RoleName = @Rolename
			AND R.OrgPartyID = @PartyId

			EXEC [Security].[SetDefaultRole] @SecurityRoleId,@CreatedBy
		END
     END;
CREATE PROCEDURE [Enterprise].[DeleteRole](@RoleId INT)
AS
     BEGIN
        DECLARE @ErrorLogID INT;
		 Declare @OrgPartyId INT,@SecurityRoleId INT
		 DECLARE @SchemaName varchar(25);
		 DECLARE @Rolename varchar(256)
		
		Select @Rolename = RV.Value, @OrgPartyId = R.PartyID 
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
    --validate if role is assinged to user.
         IF EXISTS
         (
             SELECT 1
             FROM Enterprise.PersonaPrivilege
             WHERE RoleId = @RoleId
         )
             BEGIN
                 SELECT @RoleId AS RoleId,
                        'Role cannot be deleted because it is currently assigned to users.' AS ErrorMessage;
                 RETURN;
         END;

		 IF EXISTS
         (
             SELECT 1
             FROM Enterprise.Role
             WHERE RoleId = @RoleId AND DefaultRole = 1
         )
             BEGIN
                 SELECT @RoleId AS RoleId,
                        'Role cannot be deleted because it is currently marked as default role.' AS ErrorMessage;
                 RETURN;
         END;
    
    --Delete all the rights associated with the role
         IF EXISTS
         (
             SELECT 1
             FROM Enterprise.[Right]
             WHERE RoleId = @RoleID
         )
             BEGIN
                 DELETE FROM Enterprise.[Right]
                 WHERE RoleId = @RoleId;
         END;
	    --Delete Role
         IF EXISTS
         (
             SELECT 1
             FROM Enterprise.Role
             WHERE RoleId = @RoleID
         )
             BEGIN TRY
                 DELETE FROM Enterprise.Role
                 WHERE RoleId = @RoleID;
         END TRY
             BEGIN CATCH
                 EXEC dbo.LogError
                      @ErrorLogID = @ErrorLogID OUTPUT;
                 SELECT 0 AS Id,
                        ErrorMessage
                 FROM dbo.ErrorLog
                 WHERE ErrorLogID = @ErrorLogID;
         END CATCH;
             ELSE
             BEGIN
                 SELECT @RoleId AS RoleId,
                        'Role does not exist.';
         END;
		--delete from security schema 
		IF (@SchemaName = 'Enterprise')
		BEGIN
			
			Select @SecurityRoleId = R.RoleID from Security.Role R 							
			Where R.RoleName = @Rolename
			AND R.OrgPartyID = @OrgPartyId

			EXEC [Security].[DeleteRole] @SecurityRoleId
		END
     END;
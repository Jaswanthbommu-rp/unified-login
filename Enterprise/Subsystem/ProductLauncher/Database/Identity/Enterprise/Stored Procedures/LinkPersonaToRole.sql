CREATE PROCEDURE [Enterprise].[LinkPersonaToRole]
(@PersonaID         BIGINT,
 @RoleID            INT,
 @IsDeleted         BIT    = 0,
 @CreatedBy nvarchar(50) NULL,
 @PersonaPrivilgeID INT OUTPUT
)
AS
     BEGIN
        DECLARE @ErrorLogID INT;
		 DECLARE @SchemaName varchar(25);
		 Declare @OrgPartyId INT;
		 Declare @SecurityRoleId INT
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

         IF @IsDeleted = 0
             BEGIN
                 IF NOT EXISTS
                 (
                     SELECT 1
                     FROM Enterprise.PersonaPrivilege
                     WHERE PersonaId = @PersonaID
                           AND RoleID = @RoleID
                 )
                     BEGIN
                         BEGIN TRY
                             INSERT INTO Enterprise.PersonaPrivilege
                             (PersonaId,
                              RoleID,
                              FromDate
                             )
                             VALUES
                             (@PersonaID,
                              @RoleID,
                              GETUTCDATE()
                             );
                             SELECT @PersonaPrivilgeID = SCOPE_IDENTITY();
                             SELECT @PersonaPrivilgeID AS Id,
                                    '' AS ErrorMessage;
                 END TRY
                         BEGIN CATCH
                             EXEC dbo.LogError
                                  @ErrorLogID = @ErrorLogID OUTPUT;
                             SELECT 0 AS Id,
                                    ErrorMessage
                             FROM dbo.ErrorLog
                             WHERE ErrorLogID = @ErrorLogID;
                 END CATCH;
                 END;
                     ELSE
                     BEGIN
                         SELECT 'PersonaID is already assigned with this role.';
                 END;
         END;
         IF @IsDeleted = 1
             BEGIN
                 IF EXISTS
                 (
                     SELECT 1
                     FROM Enterprise.PersonaPrivilege
                     WHERE PersonaId = @PersonaID
                           AND RoleID = @RoleID
                 )
                 BEGIN
                         DECLARE @DeletedPersonaPrivilegeId TABLE(PersonaPrivilegeId INT);
                 END;
                 BEGIN TRY
                     DELETE FROM Enterprise.PersonaPrivilege
                     OUTPUT DELETED.UserPrivilegeId
                            INTO @DeletedPersonaPrivilegeId
                     WHERE PersonaId = @PersonaId
                           AND RoleId = @RoleId;
                     SELECT PersonaPrivilegeId AS 'Id',
                            '' AS ErrorMessage
                     FROM @DeletedPersonaPrivilegeId;
				END TRY
                 BEGIN CATCH
                     EXEC dbo.LogError
                          @ErrorLogID = @ErrorLogID OUTPUT;
                     SELECT 0 AS Id,
                            ErrorMessage
                     FROM dbo.ErrorLog
                     WHERE ErrorLogID = @ErrorLogID;
             END CATCH;
         END;
		 --link persona to role in security schema 
		IF (@SchemaName = 'Enterprise')
		BEGIN
			Declare @PersonaRoleID INT 
			Select @SecurityRoleId = R.RoleID from Security.Role R 							
			Where R.RoleName = @Rolename
			AND R.OrgPartyID = @OrgPartyId

			EXEC [Security].[LinkPersonaToRole] @PersonaId,@SecurityRoleId,@IsDeleted , @CreatedBy,@PersonaRoleID OUTPUT
		END
     END;


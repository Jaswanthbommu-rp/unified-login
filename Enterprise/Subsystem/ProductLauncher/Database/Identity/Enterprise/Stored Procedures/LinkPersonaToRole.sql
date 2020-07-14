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
     END;


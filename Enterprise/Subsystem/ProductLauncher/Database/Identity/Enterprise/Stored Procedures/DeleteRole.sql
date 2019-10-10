CREATE PROCEDURE [Enterprise].[DeleteRole](@RoleId INT)
AS
     BEGIN
         DECLARE @ErrorLogID INT;
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
     END;
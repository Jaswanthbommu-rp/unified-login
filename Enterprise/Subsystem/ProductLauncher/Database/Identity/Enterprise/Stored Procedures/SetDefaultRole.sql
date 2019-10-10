CREATE PROCEDURE [Enterprise].[SetDefaultRole](@RoleId INT)
AS
     BEGIN
         DECLARE @DefaultRoleId INT;
         DECLARE @PartyId INT;
         SELECT @PartyId = R.PartyId
         FROM Enterprise.Role R
              INNER JOIN Enterprise.RoleValueType RR ON RR.RoleValueTypeId = R.RoleValueTYpeId
         WHERE R.RoleId = @RoleId;
         
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
     END;
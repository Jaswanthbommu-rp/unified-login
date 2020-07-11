CREATE PROCEDURE [Security].[SetDefaultRole](@RoleId INT,@CreatedBy nvarchar(50))
AS
     BEGIN
         DECLARE @DefaultRoleId INT;
         DECLARE @PartyId INT;
         
		 SELECT @PartyId = R.OrgPartyID
         FROM Security.Role R             
         WHERE R.RoleId = @RoleId;
         
		 IF @PartyId IS NOT NULL
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

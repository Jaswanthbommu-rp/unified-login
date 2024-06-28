CREATE PROCEDURE [Enterprise].[InsertUpdateSuperVisor]
	@UserId BIGINT, 
	@SuperVisorUserId BIGINT
AS
BEGIN
	BEGIN TRY
		IF NOT EXISTS(SELECT 1 FROM Enterprise.UserSuperVisor WHERE UserId = @UserId)
		BEGIN
			INSERT INTO Enterprise.UserSuperVisor(UserId, SuperVisorUserId)
			OUTPUT Inserted.SuperVisorId AS Id, '' AS ErrorMessage
			VALUES(@UserId, @SuperVisorUserId)
		END
		ELSE
		BEGIN
			UPDATE Enterprise.UserSuperVisor SET SuperVisorUserId = @SuperVisorUserId
			WHERE UserId = @UserId
			SELECT @SuperVisorUserId as Id, '' AS ErrorMessage
		END
	END TRY  
	BEGIN CATCH  
		DECLARE @ErrorLogID INT;  
		EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;  
  
		SELECT 0 AS Id ,ErrorMessage  
		FROM   dbo.ErrorLog  
		WHERE  ErrorLogID = @ErrorLogID;  
	END CATCH;
END
GO
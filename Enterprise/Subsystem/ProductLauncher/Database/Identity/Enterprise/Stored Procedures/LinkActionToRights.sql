CREATE PROCEDURE [Enterprise].[LinkActionToRights]
( 
				@ActionID int, @RightId int, @StatusId int, @UserActionId int OUTPUT
)
AS
BEGIN
	BEGIN TRY
		IF NOT EXISTS
		(
			SELECT 1
			FROM Enterprise.UserActions
			WHERE ActionId = @ActionId AND 
				  RightId = @RightId
		)
		BEGIN
			INSERT INTO Enterprise.UserActions( ActionID, RightId, Status )
			VALUES( @ActionID, @RightId, @StatusId );
			SELECT @UserActionId = SCOPE_IDENTITY();
			SELECT @UserActionId AS 'UserActionId', '' AS ErrorMessage;
		END;
		ELSE
		BEGIN
			SELECT UserActionId, '' AS ErrorMessage
			FROM ENterprise.UserActions
			WHERE RightId = @RightId AND 
				  ActionId = @ActionId;
		END;
	END TRY
	BEGIN CATCH
		DECLARE @ErrorLogID int;
		EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;
		SELECT 0 AS Id, ErrorMessage
		FROM dbo.ErrorLog
		WHERE ErrorLogID = @ErrorLogID;
	END CATCH;
END;
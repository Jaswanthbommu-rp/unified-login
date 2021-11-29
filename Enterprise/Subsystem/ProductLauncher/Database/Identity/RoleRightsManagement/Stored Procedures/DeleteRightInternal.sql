CREATE PROCEDURE [Security].[DeleteRightInternal](@RightId INT)
AS
	BEGIN
		DECLARE @ErrorLogID INT;
		IF EXISTS
		     (
		         SELECT 1
		         FROM Security.[Right]
		         WHERE RightId = @RightId
		     )
		     BEGIN TRY
		             DELETE FROM Security.[Right]
		             WHERE RightId = @RightId;
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
		             SELECT @RightId AS RightId,
		                    'Right does not exist.';
		     END;
	END;

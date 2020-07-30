CREATE PROCEDURE [Enterprise].[UpdateUserEmployeeId](
	 @UserEmployeeId INT,
	 @UserLoginPersonaId BIGINT = NULL,
	 @EmployeeId NVARCHAR(40) = NULL
	) 
AS
BEGIN
	SET NOCOUNT ON;
	 BEGIN TRY
            BEGIN TRAN;
				UPDATE [Enterprise].[UserEmployeeId]
				   SET 
					   Employee = @EmployeeId
					WHERE
						UserEmployeeId = @UserEmployeeId

				SELECT @UserEmployeeId as Id, '' AS ErrorMessage
			COMMIT;
        END TRY
        BEGIN CATCH
            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT 0 AS Id ,
                   ErrorMessage
            FROM   dbo.ErrorLog
            WHERE  ErrorLogID = @ErrorLogID;

            ROLLBACK;
        END CATCH;
END;
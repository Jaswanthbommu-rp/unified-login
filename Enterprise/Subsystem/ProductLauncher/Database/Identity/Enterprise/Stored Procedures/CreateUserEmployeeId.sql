CREATE PROCEDURE [Enterprise].[CreateUserEmployeeId]
	 @UserLoginPersonaId BIGINT = NULL,
	 @EmployeeId NVARCHAR(40) = NULL
AS
    BEGIN
        BEGIN TRY
            BEGIN TRAN;
			INSERT INTO Enterprise.UserEmployeeId (
                UserLoginPersonaId ,
                Employee
				)
			OUTPUT Inserted.UserEmployeeId AS Id, '' AS ErrorMessage
			VALUES (
                     @UserLoginPersonaId ,
					 @EmployeeId
                 );
            COMMIT;
        END TRY
        BEGIN CATCH
            DECLARE @ErrorLogID INT;
        EXEC dbo.LogError
			@ErrorLogID = @ErrorLogID OUTPUT;

            SELECT 0 AS Id ,
                   ErrorMessage
            FROM   dbo.ErrorLog
            WHERE  ErrorLogID = @ErrorLogID;

            ROLLBACK;
        END CATCH;
    END;
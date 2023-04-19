
CREATE PROCEDURE [Enterprise].[DeleteContactMechanismDefault] (
	@ContactMechanismId INT
)
AS
BEGIN
    BEGIN TRY
        DELETE Enterprise.ContactMechanismDefault 
			WHERE ContactMechanismID = @ContactMechanismId

		SELECT	1 AS Id,
                '' AS ErrorMessage
    END TRY
    BEGIN CATCH
        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id ,
                ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;


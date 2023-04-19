
CREATE PROCEDURE [Enterprise].[AddUpdateContactMechanismDefault] (
	@CurrentContactMechanismId INT,
	@PreviousPreferenceId  INT
)
AS
BEGIN
    BEGIN TRY
		IF (@PreviousPreferenceId <> 0 and @PreviousPreferenceId <> @CurrentContactMechanismId)
		BEGIN
			UPDATE  t
			SET    t.ContactMechanismID = @CurrentContactMechanismId
			FROM    Enterprise.ContactMechanismDefault t
			WHERE   ContactMechanismID = @PreviousPreferenceId
		END
		ELSE
		BEGIN
			INSERT INTO Enterprise.ContactMechanismDefault(
				ContactMechanismID
				)
			VALUES(
				@CurrentContactMechanismId
			)
		END
		SELECT	@CurrentContactMechanismId AS Id,
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

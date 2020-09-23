-----------------------------------------------------------------------------
-- procedure  : [Enterprise].[AddUpdateContactMechanismPreference]
--
-- purpose    : Used to Add/Update contact preference
-- parameters : @CurrentContactMechanismId			-- Holds ContactMechanismId
-- parameters : @PreviousPreferenceId				-- Holds Previous ContactMechanismId

--	Date		Name					Comment
-----------------------------------------------------------------------------
-- 9/22/2020	RohithVundyala			Created
-----------------------------------------------------------------------------
--
-- Copyright  : copyright (c) 2000.  RealPage Inc.
--              This module is the confidential & proprietary property of
--              RealPage Inc.
-----------------------------------------------------------------------------
CREATE PROCEDURE [Enterprise].[AddUpdateContactMechanismPreference] (
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
			FROM    Enterprise.contactmechanismPreference t
			WHERE   ContactMechanismID = @PreviousPreferenceId
		END
		ELSE
		BEGIN
			INSERT INTO Enterprise.contactmechanismPreference(
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

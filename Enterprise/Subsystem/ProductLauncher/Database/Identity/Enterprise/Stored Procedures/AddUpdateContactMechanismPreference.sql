IF EXISTS (SELECT top 1 1 FROM sys.procedures where object_id =object_id('[Enterprise].[AddUpdateContactMechanismPreference]'))
DROP PROCEDURE [Enterprise].[AddUpdateContactMechanismPreference]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-----------------------------------------------------------------------------
-- procedure  : [Enterprise].[AddUpdateContactMechanismPreference]
--
-- purpose    : Used to Add/Update contact preference
-- parameters : @ContactMechanismId			-- Holds ContactMechanismId
-- parameters : @PreviousePreferenceId      -- Holds Previous ContactMechanismId

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
	@CurrrentContactMechanismId INT,
	@PreviousePreferenceId INT
)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION; 
		IF (@PreviousePreferenceId <> 0 and @PreviousePreferenceId <> @CurrrentContactMechanismId)
		BEGIN
			UPDATE  t
			SET    t.ContactMechanismID = @CurrrentContactMechanismId
			FROM    Enterprise.contactmechanismPreference t
			WHERE   ContactMechanismID = @PreviousePreferenceId
		END
		ELSE
		BEGIN
			INSERT INTO Enterprise.contactmechanismPreference(
				ContactMechanismID
				)
			VALUES(
				@CurrrentContactMechanismId
			)
		END
		SELECT	@CurrrentContactMechanismId AS Id,
                '' AS ErrorMessage
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id ,
                ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;

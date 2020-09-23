-----------------------------------------------------------------------------
-- procedure  : [Enterprise].[DeleteContactMechanismPreference]
--
-- purpose    : Used to Delete contact preference
-- parameters : @ContactMechanismId       -- Holds ContactMechanismId

--	Date		Name					Comment
-----------------------------------------------------------------------------
-- 9/22/2020	RohithVundyala			Created
-----------------------------------------------------------------------------
--
-- Copyright  : copyright (c) 2000.  RealPage Inc.
--              This module is the confidential & proprietary property of
--              RealPage Inc.
-----------------------------------------------------------------------------
CREATE PROCEDURE [Enterprise].[DeleteContactMechanismPreference] (
	@ContactMechanismId INT
)
AS
BEGIN
    BEGIN TRY
        DELETE Enterprise.ContactMechanismPreference 
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


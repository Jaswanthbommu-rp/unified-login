IF OBJECT_ID('[Person].[GetPartyRole]') IS NOT NULL
	DROP PROCEDURE [Person].[GetPartyRole];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Person].[GetPartyRole]
	@PartyId BIGINT
AS
    BEGIN
        BEGIN TRY
        -- Check if the ContactMechanism Exists, If it does, then update it.

			SELECT PartyRoleId ,
                   PartyId ,
                   RoleTypeId
			FROM Enterprise.PartyRole
			WHERE PartyId = @PartyId

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
GO

CREATE PROCEDURE [Person].[SetPartyRole]
    @PartyId BIGINT ,
    @RoleTypeId INT
AS
    BEGIN
        BEGIN TRY
        -- Check if the ContactMechanism Exists, If it does, then update it.

            BEGIN TRANSACTION; 

            UPDATE  r
            SET     r.RoleTypeId = @RoleTypeId
            OUTPUT  Inserted.PartyRoleId AS Id ,
                    '' AS ErrorMessage
            FROM    Enterprise.PartyRole r
            WHERE   r.PartyId = @PartyId;

            IF @@ROWCOUNT = 0
                BEGIN

                    INSERT  INTO Enterprise.PartyRole
                            ( PartyId, RoleTypeId )
                    OUTPUT  Inserted.PartyRoleId AS Id, '' AS ErrorMessage
                    VALUES  ( @PartyId, @RoleTypeId );
	    
                END;

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


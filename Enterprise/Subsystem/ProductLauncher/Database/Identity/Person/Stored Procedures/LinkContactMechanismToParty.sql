CREATE PROCEDURE Person.LinkContactMechanismToParty (
	@RealPageId UNIQUEIDENTIFIER,
	@ContactMechanismId INT,
	@FromDate DATETIME,
	@ThruDate DATETIME = NULL,
	@PartyContactMechanismId bigint = NULL
)
AS
BEGIN
    BEGIN TRY
        DECLARE @PartyID BIGINT;

        SELECT  @PartyID = p.PartyId
        FROM    Enterprise.Party p
        WHERE   p.RealPageId = @RealPageId;		

        BEGIN TRANSACTION; 	

		-- Check to see if we want to expire a current Contact Method
        IF ((@PartyContactMechanismId IS NOT NULL) AND (@PartyContactMechanismId > 0))
        BEGIN
            UPDATE  Enterprise.PartyContactMechanism
            SET     ThruDate = GETUTCDATE()
			OUTPUT  Inserted.PartyContactMechanismId AS Id ,
					@RealPageId AS RealPageId ,
					'' AS ErrorMessage
			WHERE   PartyContactMechanismId = @PartyContactMechanismId; 
        END;
        ELSE
        BEGIN
            INSERT  INTO Enterprise.PartyContactMechanism (
				PartyId ,
				ContactMechanismId ,
				FromDate ,
				ThruDate
            )
            OUTPUT  Inserted.PartyContactMechanismId AS Id ,
                    @RealPageId AS RealPageId ,
                    '' AS ErrorMessage
            VALUES  (
				@PartyID ,
				@ContactMechanismId ,
				@FromDate ,
				@ThruDate
			);
        END;

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id ,
                @RealPageId AS RealPageId ,
                ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
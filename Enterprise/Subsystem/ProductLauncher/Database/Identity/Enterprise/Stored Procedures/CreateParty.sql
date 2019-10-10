CREATE PROCEDURE [Enterprise].[CreateParty]
	@PartyID BIGINT OUTPUT,
	@RealPageId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
	BEGIN TRY
		DECLARE @PartyResult AS TABLE
            (
                PartyId INT ,
                RealPageId UNIQUEIDENTIFIER
            );
		
        INSERT  INTO Enterprise.Party
        OUTPUT  Inserted.PartyId ,
                Inserted.RealPageId
                INTO @PartyResult
                DEFAULT VALUES;

        SELECT  @PartyId = PartyId,
				@RealPageId = RealPageId
        FROM    @PartyResult;
	END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				'' AS RealPageId,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
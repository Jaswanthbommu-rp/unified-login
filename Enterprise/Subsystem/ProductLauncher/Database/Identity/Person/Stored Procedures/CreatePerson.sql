CREATE PROCEDURE [Person].[CreatePerson]
    @Title NVARCHAR(50) = NULL ,
    @FirstName Name ,
    @MiddleName Name = NULL ,
    @LastName Name ,
    @Suffix NVARCHAR(10) = NULL,
	@PreferredContactMethodId int = NULL,
	@RealPageId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    BEGIN TRY
        DECLARE @PartyResult AS TABLE
            (
                PartyId INT ,
                RealPageId UNIQUEIDENTIFIER
            );
		
        DECLARE @PartyId bigint;

        BEGIN TRANSACTION; 
        INSERT  INTO Enterprise.Party
        OUTPUT  Inserted.PartyId ,
                Inserted.RealPageId
                INTO @PartyResult
                DEFAULT VALUES;

        SELECT  @PartyId = PartyId
        FROM    @PartyResult;

        INSERT  INTO Person.Person
                ( PartyId ,
                    Title ,
                    FirstName ,
                    MiddleName ,
                    LastName ,
                    Suffix ,
					PreferredContactMethodId
				)
        VALUES  ( @PartyId ,
                    @Title ,
                    @FirstName ,
                    @MiddleName ,
                    @LastName ,
                    @Suffix  ,
					@PreferredContactMethodId
				);

		SELECT	PartyId AS Id,
				RealPageId,
				'' AS ErrorMessage
        FROM    @PartyResult;

		SELECT @RealPageId = RealPageId
		FROM @PartyResult

        COMMIT;
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
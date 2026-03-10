CREATE PROCEDURE [Enterprise].[InsertCompanyAddress]
(
    @CompanyPartyId BIGINT,
    @Address VARCHAR(255),
    @City VARCHAR(100),
    @State VARCHAR(50),
    @PostalCode VARCHAR(20),
    @County VARCHAR(100),
    @Country VARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ErrorMessage NVARCHAR(4000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    BEGIN TRY
        -- Input validation
        IF @CompanyPartyId IS NULL OR @CompanyPartyId <= 0
        BEGIN
            RAISERROR('Invalid CompanyPartyId: Must be a positive integer', 16, 1);
            RETURN;
        END;

        BEGIN TRANSACTION;

        -- Use MERGE for atomic upsert operation
        MERGE [Enterprise].[OrganizationAddress] AS target
        USING (
            SELECT 
                @CompanyPartyId AS CompanyPartyId,
                @Address AS Address,
                @City AS City,
                @State AS State,
                @PostalCode AS PostalCode,
                @County AS County,
                @Country AS Country,
                GETUTCDATE() AS CurrentDate
        ) AS source
        ON target.[CompanyPartyId] = source.CompanyPartyId
        WHEN MATCHED THEN
            UPDATE SET
                target.[Address] = source.Address,
                target.[City] = source.City,
                target.[State] = source.State,
                target.[PostalCode] = source.PostalCode,
                target.[County] = source.County,
                target.[Country] = source.Country,
                target.[ModifiedDate] = source.CurrentDate
        WHEN NOT MATCHED THEN
            INSERT (
                [CompanyPartyId],
                [Address],
                [City],
                [State],
                [PostalCode],
                [County],
                [Country],
                [CreatedDate]
            )
            VALUES (
                source.CompanyPartyId,
                source.Address,
                source.City,
                source.State,
                source.PostalCode,
                source.County,
                source.Country,
                source.CurrentDate
            );

        COMMIT TRANSACTION;

        -- Return success result
        SELECT 
            @CompanyPartyId AS Id, 
            '' AS ErrorMessage,
            @@ROWCOUNT AS RowsAffected;

    END TRY
    BEGIN CATCH
        -- Rollback transaction if active
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        -- Capture error details
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        -- Return error result
        SELECT 
            0 AS Id, 
            @ErrorMessage AS ErrorMessage,
            0 AS RowsAffected;

    END CATCH;

END;
GO
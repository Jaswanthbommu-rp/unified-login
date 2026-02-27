CREATE PROCEDURE [Enterprise].[BulkCreateDeleteUPFMPropertyInstanceMapping]
    @PersonaID BIGINT,
    @ProductID INT,
    @PropertyMappings [Enterprise].[UPFMPropertyInstanceMapping] READONLY
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SuccessCount INT = 0;
    DECLARE @ErrorCount INT = 0;
    DECLARE @ErrorMessage NVARCHAR(MAX) = '';
    DECLARE @Now DATETIME = GETUTCDATE(); 

    BEGIN TRANSACTION; -- Added transaction control
    
    BEGIN TRY
        -- Soft delete records marked as deleted
        UPDATE pim
        SET 
            ThruDate = @Now,  
            Active = 0
        FROM Enterprise.PropertyInstanceMapping pim
            INNER JOIN @PropertyMappings pm ON pim.PropertyInstanceID = pm.PropertyInstanceID
        WHERE pim.PersonaID = @PersonaID
            AND pim.ProductID = @ProductID
            AND pim.Active = 1
            AND pm.IsDeleted = 1;
        
        SET @SuccessCount = @SuccessCount + @@ROWCOUNT;

        -- Process insertions
        INSERT INTO Enterprise.PropertyInstanceMapping (PersonaID, ProductID, PropertyInstanceID)
        SELECT @PersonaID, @ProductID, pm.PropertyInstanceID
        FROM @PropertyMappings pm
        WHERE pm.IsDeleted = 0
          AND NOT EXISTS (
              SELECT 1
              FROM Enterprise.PropertyInstanceMapping pim
              WHERE pim.PersonaID = @PersonaID
                AND pim.ProductID = @ProductID
                AND pim.PropertyInstanceID = pm.PropertyInstanceID
                AND pim.Active = 1  -- Added table alias for clarity
          );
        
        SET @SuccessCount = @SuccessCount + @@ROWCOUNT;

        COMMIT TRANSACTION; -- Commit if all successful
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION; -- Rollback on any error
            
        SET @ErrorMessage = ERROR_MESSAGE();
        SET @ErrorCount = 1;
        
        -- Re-throw error to calling code
        THROW;
        
    END CATCH

    -- Return summary (only reached if successful)
    SELECT 
        @SuccessCount AS SuccessCount,
        @ErrorCount AS ErrorCount,
        @ErrorMessage AS ErrorMessage;
END
GO
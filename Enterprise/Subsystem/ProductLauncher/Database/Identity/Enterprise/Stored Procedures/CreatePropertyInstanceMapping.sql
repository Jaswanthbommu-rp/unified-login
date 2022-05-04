CREATE PROCEDURE [Enterprise].[CreatePropertyInstanceMapping](
    @PersonaID  BIGINT,
    @ProductID  INT,
    @PropertyInstanceID BIGINT,
    @Deleted    BIT    = 0
)
AS
     BEGIN
         DECLARE @PropertyInstanceMappingID BIGINT;
         DECLARE @Now DATETIME = GETUTCDATE();
         IF @Deleted = 0
             BEGIN
                 IF NOT EXISTS
                 (
                     SELECT 1
                     FROM Enterprise.PropertyInstanceMapping
                     WHERE PersonaID = @PersonaID
                           AND ProductId = @ProductID
                           AND PropertyInstanceId = @PropertyInstanceID
                           AND Active = 1
                 )
                     BEGIN TRY
                         INSERT INTO Enterprise.PropertyInstanceMapping
                         ( PersonaId,
                           PropertyInstanceId,
                           ProductId
                         )
                         VALUES
                         (@PersonaID,
                          @PropertyInstanceID,
                          @ProductID
                         );
                         SELECT @PropertyInstanceMappingID = SCOPE_IDENTITY();
                         SELECT @PropertyInstanceMappingID AS PropertyInstanceMappingID,
                                '' AS ErrorMessage;
                 END TRY
                     BEGIN CATCH
                         DECLARE @ErrorLogID INT;
                         EXEC dbo.LogError
                              @ErrorLogID = @ErrorLogID OUTPUT;
                         SELECT 0 AS Id,
                                ErrorMessage
                         FROM dbo.ErrorLog
                         WHERE ErrorLogID = @ErrorLogID;
                 END CATCH;
                     ELSE
                     BEGIN
                         SELECT 'Property instance mapping already exists for the Persona.';
                 END;
         END;
         IF @Deleted = 1
             BEGIN
                 IF EXISTS
                 (
                     SELECT 1
                     FROM Enterprise.PropertyInstanceMapping
                     WHERE PersonaID = @PersonaID
                           AND ProductId = @ProductID
                           AND PropertyInstanceId = @PropertyInstanceId
                           AND Active = 1
                 )
                     BEGIN
                         UPDATE Enterprise.PropertyInstanceMapping
                           SET
                               Thrudate = @Now,
                               Active = 0
                         WHERE PersonaID = @PersonaID
                               AND ProductId = @ProductID
                               AND PropertyInstanceId = @PropertyInstanceId
                               AND Active = 1
                         SELECT 1 AS PropertyInstanceMappingID,
                                '' AS ErrorMessage;
                 END;
         END;
END;
CREATE PROCEDURE [Enterprise].[CreatePropertyMapping]
(@PersonaID  BIGINT,
 @ProductID  INT,
 @PropertyID BIGINT,
 @Deleted    BIT    = 0
)
AS
     BEGIN
         DECLARE @PropertyMappingID BIGINT;
         DECLARE @Now DATETIME;
         SET @Now = GETUTCDATE();
         IF @Deleted = 0
             BEGIN
                 IF NOT EXISTS
                 (
                     SELECT 1
                     FROM Enterprise.PropertyMapping
                     WHERE PersonaID = @PersonaID
                           AND ProductId = @ProductID
                           AND PropertyId = @PropertyId
                           AND @NOW BETWEEN CASE
                                                WHEN FromDate > @NOW
                                                THEN @NOW
                                                ELSE FromDate
                                            END AND CASE
                                                        WHEN ThruDate IS NULL
                                                        THEN DATEADD(mm, 90, @NOW)
                                                        ELSE ThruDate
                                                    END
                 )
                     BEGIN TRY
                         INSERT INTO Enterprise.PropertyMapping
                         (PersonaId,
                          PropertyId,
                          ProductId,
                          FromDate
                         )
                         VALUES
                         (@PersonaID,
                          @PropertyID,
                          @ProductID,
                          @Now
                         );
                         SELECT @PropertyMappingID = SCOPE_IDENTITY();
                         SELECT @PropertyMappingID AS PropertyMappingID,
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
                         SELECT 'Property mapping already exists for the Persona.';
                 END;
         END;
         IF @Deleted = 1
             BEGIN
                 IF EXISTS
                 (
                     SELECT 1
                     FROM Enterprise.PropertyMapping
                     WHERE PersonaID = @PersonaID
                           AND ProductId = @ProductID
                           AND PropertyId = @PropertyId
                 )
                     BEGIN
                         UPDATE Enterprise.PropertyMapping
                           SET
                               Thrudate = @now
                         WHERE PersonaID = @PersonaID
                               AND ProductId = @ProductID
                               AND PropertyId = @PropertyId
                 END;
         END;
     END;
GO



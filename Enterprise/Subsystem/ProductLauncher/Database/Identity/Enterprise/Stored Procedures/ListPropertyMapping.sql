CREATE PROCEDURE [Enterprise].[ListPropertyMapping]
(@PersonaID BIGINT,
 @ProductID INT    = NULL
)
AS
     BEGIN
         DECLARE @now DATETIME;
         SET @now = GETUTCDATE();
         SELECT PersonaID,
                PropertyID,
                ProductID,
                FromDate,
                ThruDate
         FROM Enterprise.PropertyMapping
         WHERE PersonaID = @PersonaID
               AND ProductID = @ProductID
               AND @NOW BETWEEN CASE
                                    WHEN FromDate > @NOW
                                    THEN @NOW
                                    ELSE FromDate
                                END AND CASE
                                            WHEN ThruDate IS NULL
                                            THEN DATEADD(mm, 90, @NOW)
                                            ELSE ThruDate
                                        END;
     END;
GO
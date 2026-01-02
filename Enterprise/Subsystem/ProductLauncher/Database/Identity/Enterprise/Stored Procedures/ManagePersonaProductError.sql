CREATE PROCEDURE [Enterprise].[ManagePersonaProductError]              
(              
    @PersonaId BIGINT              
)              
AS              
BEGIN              
    BEGIN TRY              
        DECLARE @personaUsePrimaryProperties NVARCHAR(50) = '';          
          
        SELECT TOP 1 @personaUsePrimaryProperties =    
            CASE 
                WHEN BP.ProductId = 1 THEN JSON_VALUE(InputJSON, '$.OneSite.UsePrimaryProperties')     
                WHEN BP.ProductId = 6 THEN JSON_VALUE(InputJSON, '$.Lead2Lease.UsePrimaryProperties') 
                ELSE JSON_VALUE(InputJSON, '$.UsePrimaryProperties')     
            END    
        FROM Batch.BatchProcessor BP 
        INNER JOIN Enterprise.PersonaConfiguration PC 
            ON PC.PersonaId = BP.SubjectUserPersonaId 
           AND PC.ProductId = BP.ProductId    
        WHERE ISJSON(BP.InputJSON) > 0          
          AND BP.SubjectUserPersonaId = @PersonaId          
          AND BP.StatusTypeId IN (7,8) 
          AND PC.StatusTypeId IN (7,8) 
          AND BP.ProductId NOT IN (42)    
          AND PC.ThruDate IS NULL    
          AND (
                CASE 
                    WHEN BP.ProductId = 1 THEN JSON_VALUE(InputJSON, '$.OneSite.UsePrimaryProperties')       
                    WHEN BP.ProductId = 6 THEN JSON_VALUE(InputJSON, '$.Lead2Lease.UsePrimaryProperties') 
                    ELSE JSON_VALUE(InputJSON, '$.UsePrimaryProperties')       
                END
              ) = 'true'     
        ORDER BY BP.BatchProcessorId DESC;           
          
        IF @personaUsePrimaryProperties = 'true' 
           AND EXISTS (
                SELECT 1 
                FROM Enterprise.PersonaConfiguration pc              
                INNER JOIN Enterprise.OrganizationProduct op 
                    ON op.ProductId = pc.ProductId 
                   AND op.ThruDate IS NULL 
                   AND pc.ThruDate IS NULL              
                INNER JOIN [Security].[RoleTemplateProduct] rtp 
                    ON rtp.ProductId = pc.ProductId              
                INNER JOIN [Security].[RoleTemplateUserMapping] rtum 
                    ON rtum.RoleTemplateId = rtp.RoleTemplateId              
                WHERE pc.PersonaId = @PersonaId
           )              
        BEGIN              
            DELETE FROM Enterprise.PersonaProductError 
            WHERE PersonaId = @PersonaId;           
        END              
        ELSE IF EXISTS (
                SELECT 1  
                FROM Enterprise.PersonaConfiguration pc              
                INNER JOIN Enterprise.OrganizationProduct op 
                    ON op.ProductId = pc.ProductId 
                   AND op.ThruDate IS NULL 
                   AND pc.ThruDate IS NULL              
                WHERE pc.PersonaId = @PersonaId 
                  AND pc.StatusTypeId = 7
           )              
        BEGIN              
            IF NOT EXISTS (
                SELECT 1 
                FROM Enterprise.PersonaProductError 
                WHERE PersonaId = @PersonaId
            )              
            BEGIN              
                INSERT INTO Enterprise.PersonaProductError(PersonaId)              
                VALUES (@PersonaId);              
            END              
        END              
        ELSE              
        BEGIN              
            DELETE FROM Enterprise.PersonaProductError 
            WHERE PersonaId = @PersonaId;              
        END              
              
        SELECT @PersonaId AS Id, '' AS ErrorMessage;              
    END TRY              
    BEGIN CATCH                
        DECLARE @ErrorLogID INT;                
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;                
               
        SELECT 0 AS Id, ErrorMessage                
        FROM dbo.ErrorLog                
        WHERE ErrorLogID = @ErrorLogID;                
    END CATCH              
END;
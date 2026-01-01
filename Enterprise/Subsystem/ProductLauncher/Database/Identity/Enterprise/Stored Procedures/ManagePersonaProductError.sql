CREATE PROCEDURE [Enterprise].[ManagePersonaProductError]              
(              
??????@PersonaId BigInt              
)              
AS              
BEGIN              
BEGIN TRY              
              
   declare @personaUsePrimaryProperties nvarchar(50) = ''  

 select top 1 @personaUsePrimaryProperties =    
 (case when BP.ProductId = 1 then JSON_VALUE(InputJSON, '$.OneSite.UsePrimaryProperties')     
           when BP.ProductId = 6 then JSON_VALUE(InputJSON, '$.Lead2Lease.UsePrimaryProperties') else JSON_VALUE(InputJSON, '$.UsePrimaryProperties')     
     end)    
 
 FROM  Batch.BatchProcessor BP inner join Enterprise.PersonaConfiguration PC on PC.PersonaId = BP.SubjectUserPersonaId and PC.ProductId = BP.ProductId    
 WHERE ISJSON(BP.InputJSON) > 0          

 AND   BP.SubjectUserPersonaId = @PersonaId          
 AND   BP.StatusTypeId in( 7,8) AND PC.StatusTypeId in ( 7,8) and BP.ProductId not in (42)    
 AND PC.ThruDate is null    
 AND (case when BP.ProductId = 1 then JSON_VALUE(InputJSON, '$.OneSite.UsePrimaryProperties')       
           when BP.ProductId = 6 then JSON_VALUE(InputJSON, '$.Lead2Lease.UsePrimaryProperties') else JSON_VALUE(InputJSON, '$.UsePrimaryProperties')       
     end) = 'true'     
 ORDER BY BatchProcessorId DESC           
          
            
?      IF @personaUsePrimaryProperties = 'true' and EXISTS (SELECT TOP 1 1 FROM Enterprise.PersonaConfiguration pc              
??????????????????????????????inner join Enterprise.OrganizationProduct op on op.ProductId = pc.ProductId AND op.ThruDate IS NULL AND pc.ThruDate IS NULL              
??????                INNER JOIN [Security].[RoleTemplateProduct] rtp ON rtp.ProductId = pc.ProductId              
??????????????????????????????INNER JOIN [Security].[RoleTemplateUserMapping] rtum ON rtum.RoleTemplateId = rtp.RoleTemplateId              
??????????????????????????????WHERE pc.PersonaId = @PersonaId  ) ??????              
??????BEGIN              
??????              
??????DELETE FROM Enterprise.PersonaProductError WHERE PersonaId =@PersonaId           
??????END              
              
??????ELSE IF EXISTS(SELECT TOP 1 1  FROM Enterprise.PersonaConfiguration pc              
??????????????????????????????inner join Enterprise.OrganizationProduct op on op.ProductId = pc.ProductId AND op.ThruDate IS NULL AND pc.ThruDate IS NULL              
??????WHERE PersonaId = @PersonaId AND StatusTypeId = 7)              
??????BEGIN              
????????????IF NOT EXISTS(SELECT 1 FROM Enterprise.PersonaProductError WHERE PersonaId = @PersonaId)              
????????????BEGIN              
??????????????????INSERT INTO Enterprise.PersonaProductError(PersonaId)              
??????????????????select @PersonaId              
????????????END              
??????END              
??????ELSE              
??????BEGIN              
????????????DELETE FROM Enterprise.PersonaProductError WHERE PersonaId =@PersonaId              
??????END              
              
??????SELECT @PersonaId AS Id ,                
                '' AS ErrorMessage              
END TRY              
BEGIN CATCH                
        DECLARE @ErrorLogID INT;                
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;                
               
        SELECT 0 AS Id ,                
                ErrorMessage                
        FROM dbo.ErrorLog                
        WHERE ErrorLogID = @ErrorLogID;                
 END CATCH              
END
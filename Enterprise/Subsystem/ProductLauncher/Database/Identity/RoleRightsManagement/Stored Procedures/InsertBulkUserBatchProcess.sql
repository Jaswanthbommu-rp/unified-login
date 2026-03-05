
CREATE PROCEDURE Batch.InsertBulkUserBatchProcess    
    @EditorUserPersonaId BIGINT,    
    @SubjectUserPersonaIds [Enterprise].[SyncPersonaList] READONLY,    
    @ProductIds [Enterprise].[ProductIdType] READONLY,
    @UseAPIV2 BIT = 0
AS    
BEGIN    
    SET NOCOUNT ON;    
    
 DROP TABLE IF EXISTS #TempSubjectPersonaIds    
 CREATE TABLE #TempSubjectPersonaIds (Id INT IDENTITY(1,1), SubjectPersonaId BIGINT)    
 INSERT INTO #TempSubjectPersonaIds    
 SELECT PersonaId from @SubjectUserPersonaIds   
 DECLARE @Cnt int = 1, @CurrentSubjectPersonaId BIGINT = 0, @TotalRows INT = (SELECT COUNT(1) FROM #TempSubjectPersonaIds)    
    
 WHILE ( @Cnt  <= @TotalRows )    
 BEGIN    
 SET @CurrentSubjectPersonaId = ( SELECT TOP 1 SubjectPersonaId FROM #TempSubjectPersonaIds WHERE Id = @Cnt )     
    -- Insert bulk product update records    
    INSERT INTO [Batch].[BulkUserBatchProcess] (EditorUserPersonaId, SubjectUserPersonaId, BatchProcessTypeId , StatusTypeId, CreatedDateTime, UseAPIV2)    
    SELECT @EditorUserPersonaId, @CurrentSubjectPersonaId, 16, 5, GETUTCDATE(), @UseAPIV2
    
  INSERT INTO [Security].[BulkUserProducts] (BulkUserBatchProcessId,ProductId,CreatedDateTime)    
  SELECT SCOPE_IDENTITY() ,ProductId, GETUTCDATE() FROM @ProductIds    
    
  SET @Cnt = @Cnt + 1    
   END    
   select 1  
  
END;
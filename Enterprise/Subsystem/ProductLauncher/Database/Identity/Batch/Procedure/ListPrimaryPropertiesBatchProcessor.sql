CREATE PROCEDURE [Batch].[ListPrimaryPropertiesBatchProcessor] ( @BatchSize INT, @UseAPIV2 BIT = 0)
AS  
BEGIN  
  
    SET NOCOUNT ON;  
     DECLARE @PBFiltered TABLE  
    (  
        [PrimaryPropertyBatchProcessId] [BIGINT] NOT NULL,  
        [EditorUserPersonaId] [BIGINT] NOT NULL,  
        [SubjectUserPersonaId] [BIGINT] NOT NULL,           
        [StatusTypeId] [INT] NOT NULL,  
        [CreatedDateTime] [DATETIME] NOT NULL,  
        [BatchProcessTypeId] [TINYINT] NOT NULL  
    );  

    BEGIN TRANSACTION; -- HAve to lock the tables so that another process can't come in and scoop up our waiting processes  
  
 ;with batchtoprocess as (  
  SELECT  
      [PrimaryPropertyBatchProcessId],  
      [EditorUserPersonaId],  
      [SubjectUserPersonaId],  
      [StatusTypeId],  
      [CreatedDateTime],  
      [BatchProcessTypeId],
      row_number() over (partition by subjectuserpersonaid order by PrimaryPropertyBatchProcessId asc ) as rn,  
      row_number() over (partition by editoruserpersonaid order by PrimaryPropertyBatchProcessId asc ) as rn2    
  FROM Batch.[PrimaryPropertiesBatchProcess] BP  
  WHERE BP.StatusTypeID = 5 AND bp.createddatetime > dateadd(dd, -3, getutcdate())
        AND BP.UseAPIV2 = @UseAPIV2
  )  
 
    INSERT INTO @PBFiltered  
    (  
      [PrimaryPropertyBatchProcessId],  
      [EditorUserPersonaId],  
      [SubjectUserPersonaId] ,  
      [StatusTypeId],  
      [CreatedDateTime],  
      [BatchProcessTypeId]
    )  
 SELECT TOP (@BatchSize)  
      [PrimaryPropertyBatchProcessId],  
      [EditorUserPersonaId],  
      [SubjectUserPersonaId] ,  
      [StatusTypeId],  
      [CreatedDateTime],  
      [BatchProcessTypeId]
  From batchtoprocess   
  WHERE   rn = 1   
  And    rn2 <= 5  
  
    UPDATE Batch.PrimaryPropertiesBatchProcess  
    SET StatusTypeId = 6 --Running  
    FROM Batch.PrimaryPropertiesBatchProcess BP  
        JOIN @PBFiltered F  
            ON F.[PrimaryPropertyBatchProcessId] = BP.[PrimaryPropertyBatchProcessId];  
  
    SELECT [PrimaryPropertyBatchProcessId],  
      [EditorUserPersonaId],  
      [SubjectUserPersonaId] , 
      [StatusTypeId],  
      [CreatedDateTime],  
      [BatchProcessTypeId]
    FROM @PBFiltered;  
  
    COMMIT TRANSACTION;  

END

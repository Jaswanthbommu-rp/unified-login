CREATE PROCEDURE [Batch].[ListEnterpriseRoleBatchProcessor] ( @BatchSize INT, @UseAPIV2 BIT = 0)
AS  
BEGIN  
  
    SET NOCOUNT ON;  
     DECLARE @PBFiltered TABLE  
    (  
        [EnterpriseRoleBatchProcessId] [BIGINT] NOT NULL,  
        [EditorUserPersonaId] [BIGINT] NOT NULL,  
        [SubjectUserPersonaId] [BIGINT] NOT NULL,  
        [EnterpriseRoleTemplateId] [INT] NOT NULL,  
        [StatusTypeId] [INT] NOT NULL,  
        [CreatedDateTime] [DATETIME] NOT NULL,  
        [BatchProcessTypeId] [TINYINT] NOT NULL  
    );  

    BEGIN TRANSACTION; -- HAve to lock the tables so that another process can't come in and scoop up our waiting processes  
  
 ;with batchtoprocess as (  
  SELECT  
      [EnterpriseRoleBatchProcessId],  
      [EditorUserPersonaId],  
      [SubjectUserPersonaId],  
      [EnterpriseRoleTemplateId],  
      [StatusTypeId],  
      [CreatedDateTime],  
      [BatchProcessTypeId],
      row_number() over (partition by subjectuserpersonaid, EnterpriseRoleTemplateId order by EnterpriseRoleBatchProcessId asc ) as rn,  
      row_number() over (partition by editoruserpersonaid order by EnterpriseRoleBatchProcessId asc ) as rn2    
  FROM Batch.[EnterpriseRoleBatchProcess] BP  
  WHERE BP.StatusTypeID = 5 AND bp.createddatetime > dateadd(dd, -3, getutcdate())
        AND BP.UseAPIV2 = @UseAPIV2
  )  
 
    INSERT INTO @PBFiltered  
    (  
      [EnterpriseRoleBatchProcessId],  
      [EditorUserPersonaId],  
      [SubjectUserPersonaId] ,  
      [EnterpriseRoleTemplateId],  
      [StatusTypeId],  
      [CreatedDateTime],  
      [BatchProcessTypeId]
    )  
 SELECT TOP (@BatchSize)  
      [EnterpriseRoleBatchProcessId],  
      [EditorUserPersonaId],  
      [SubjectUserPersonaId] ,  
      [EnterpriseRoleTemplateId],  
      [StatusTypeId],  
      [CreatedDateTime],  
      [BatchProcessTypeId]
  From batchtoprocess   
  WHERE   rn = 1   
  And    rn2 <= 5  
  
    UPDATE Batch.EnterpriseRoleBatchProcess  
    SET StatusTypeId = 6 --Running  
    FROM Batch.EnterpriseRoleBatchProcess BP  
        JOIN @PBFiltered F  
            ON F.[EnterpriseRoleBatchProcessId] = BP.[EnterpriseRoleBatchProcessId];  
  
    SELECT [EnterpriseRoleBatchProcessId],  
      [EditorUserPersonaId],  
      [SubjectUserPersonaId] ,  
      [EnterpriseRoleTemplateId],  
      [StatusTypeId],  
      [CreatedDateTime],  
      [BatchProcessTypeId]
    FROM @PBFiltered;  
  
    COMMIT TRANSACTION;  

END


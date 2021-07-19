CREATE PROCEDURE [Batch].[ListEnterpriseRoleBatchProcessor] ( @BatchSize INT)
AS  
BEGIN  
  
    SET NOCOUNT ON;  
     DECLARE @PBFiltered TABLE  
    (  
        [BatchProcessEnterpriseRoleProductUpdateId] [BIGINT] NOT NULL,  
        [EditorUserPersonaId] [BIGINT] NOT NULL,  
        [SubjectUserPersonaId] [BIGINT] NOT NULL,  
        [EnterpriseRoleTemplateId] [INT] NOT NULL,  
        [StatusTypeId] [INT] NOT NULL,  
        [CreatedDateTime] [SMALLDATETIME] NOT NULL,  
        [BatchProcessTypeId] [TINYINT] NOT NULL  
    );  

    BEGIN TRANSACTION; -- HAve to lock the tables so that another process can't come in and scoop up our waiting processes  
  
 ;with batchtoprocess as (  
  SELECT  
      [BatchProcessEnterpriseRoleProductUpdateId],  
      [EditorUserPersonaId],  
      [SubjectUserPersonaId],  
      [EnterpriseRoleTemplateId],  
      [StatusTypeId],  
      [CreatedDateTime],  
      [BatchProcessTypeId],
      row_number() over (partition by subjectuserpersonaid, EnterpriseRoleTemplateId order by BatchProcessEnterpriseRoleProductUpdateId asc ) as rn,  
      row_number() over (partition by editoruserpersonaid order by BatchProcessEnterpriseRoleProductUpdateId asc ) as rn2    
  FROM Batch.[BatchProcessEnterpriseRoleProductUpdate] BP  
  WHERE BP.StatusTypeID = 5  )  
 
    INSERT INTO @PBFiltered  
    (  
      [BatchProcessEnterpriseRoleProductUpdateId],  
      [EditorUserPersonaId],  
      [SubjectUserPersonaId] ,  
      [EnterpriseRoleTemplateId],  
      [StatusTypeId],  
      [CreatedDateTime],  
      [BatchProcessTypeId]
    )  
 SELECT TOP (@BatchSize)  
      [BatchProcessEnterpriseRoleProductUpdateId],  
      [EditorUserPersonaId],  
      [SubjectUserPersonaId] ,  
      [EnterpriseRoleTemplateId],  
      [StatusTypeId],  
      [CreatedDateTime],  
      [BatchProcessTypeId]
  From batchtoprocess   
  WHERE   rn = 1   
  And    rn2 = 5  
  
    UPDATE Batch.BatchProcessEnterpriseRoleProductUpdate  
    SET StatusTypeId = 6 --Running  
    FROM Batch.BatchProcessEnterpriseRoleProductUpdate BP  
        JOIN @PBFiltered F  
            ON F.[BatchProcessEnterpriseRoleProductUpdateId] = BP.[BatchProcessEnterpriseRoleProductUpdateId];  
  
    SELECT [BatchProcessEnterpriseRoleProductUpdateId],  
      [EditorUserPersonaId],  
      [SubjectUserPersonaId] ,  
      [EnterpriseRoleTemplateId],  
      [StatusTypeId],  
      [CreatedDateTime],  
      [BatchProcessTypeId]
    FROM @PBFiltered;  
  
    COMMIT TRANSACTION;  

END

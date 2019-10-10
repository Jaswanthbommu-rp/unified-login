CREATE PROCEDURE [Batch].[ListBatchProcessConfiguration]
AS
     SET NOCOUNT ON;
     SELECT BPT.BatchProcessTypeId,
            BPT.Name AS ProcessName,
            BPC.BatchProcessConfigurationId,
            BPCT.Name AS ConfigurationTypeName,
            BPC.Value
     FROM Batch.BatchProcessConfiguration BPC
          INNER JOIN Batch.BatchProcessConfigurationType BPCT 
			ON BPC.BatchProcessConfigurationTypeId = BPCT.BatchProcessConfigurationTypeId
          INNER JOIN Batch.BatchProcessType BPT 
			ON BPC.BatchProcessConfigurationId = BPT.BatchProcessConfigurationId;
 
GO
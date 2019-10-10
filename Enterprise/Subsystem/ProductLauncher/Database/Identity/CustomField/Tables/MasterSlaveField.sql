CREATE TABLE [CustomField].[MasterSlaveField] (
    [MasterSlaveFieldId] BIGINT         IDENTITY (1, 1) NOT NULL,
    [MasterFieldId]      BIGINT         NOT NULL,
    [SlaveFieldId]       BIGINT         NOT NULL,
    [LogicalOperatorId]  TINYINT        NULL,
    [ComperatorId]       TINYINT        NOT NULL,
    [MasterFieldValue]   NVARCHAR (MAX) NULL,
    [CreatedDate]        DATETIME       NOT NULL,
    [CreatedBy]          NVARCHAR (325) NOT NULL,
    CONSTRAINT [PK_MasterSlaveField] PRIMARY KEY CLUSTERED ([MasterSlaveFieldId] ASC),
    CONSTRAINT [FK_MasterSlaveField_Comperator] FOREIGN KEY ([ComperatorId]) REFERENCES [CustomField].[Comperator] ([ComperatorId]),
    CONSTRAINT [FK_MasterSlaveField_Field] FOREIGN KEY ([MasterFieldId]) REFERENCES [CustomField].[Field] ([FieldId]),
    CONSTRAINT [FK_MasterSlaveField_Field1] FOREIGN KEY ([SlaveFieldId]) REFERENCES [CustomField].[Field] ([FieldId]),
    CONSTRAINT [FK_MasterSlaveField_LogicalOperator] FOREIGN KEY ([LogicalOperatorId]) REFERENCES [CustomField].[LogicalOperator] ([LogicalOperatorId])
);


GO

-- =============================================
-- Author:		Monte Jennings
-- Create date: 5/22/2019 1:41:42 PM
-- Description: Inserts old records edited or deleted from the MasterSlaveField table.
-- =============================================
CREATE TRIGGER [CustomField].MasterSlaveFieldAuditTrigger 
   ON  [CustomField].[MasterSlaveField] 
   AFTER DELETE,UPDATE
AS 
BEGIN
SET NOCOUNT ON;

    DECLARE @Count as int

    SELECT @Count = COUNT(*) FROM DELETED

   IF @Count > 0
		 BEGIN
			INSERT INTO Audit.[MasterSlaveField](
				 [MasterSlaveFieldId]
				,[MasterFieldId]
				,[SlaveFieldId]
				,[LogicalOperatorId]
				,[ComperatorId]
				,[MasterFieldValue]
				,[CreatedDate]
				,[CreatedBy])
			SELECT 
				 [MasterSlaveFieldId]
				,[MasterFieldId]
				,[SlaveFieldId]
				,[LogicalOperatorId]
				,[ComperatorId]
				,[MasterFieldValue]
				,[CreatedDate]
				,[CreatedBy]			
			FROM DELETED
		END
	ELSE
		BEGIN
			INSERT INTO Audit.[MasterSlaveField](
				 [MasterSlaveFieldId]
				,[MasterFieldId]
				,[SlaveFieldId]
				,[LogicalOperatorId]
				,[ComperatorId]
				,[MasterFieldValue]
				,[CreatedDate]
				,[CreatedBy])
			SELECT 
				 [MasterSlaveFieldId]
				,[MasterFieldId]
				,[SlaveFieldId]
				,[LogicalOperatorId]
				,[ComperatorId]
				,[MasterFieldValue]
				,[CreatedDate]
				,[CreatedBy]		
			FROM INSERTED
		END	
       
END


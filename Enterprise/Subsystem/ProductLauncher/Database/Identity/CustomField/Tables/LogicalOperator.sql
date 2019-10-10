CREATE TABLE [CustomField].[LogicalOperator] (
    [LogicalOperatorId] TINYINT        IDENTITY (1, 1) NOT NULL,
    [Name]              NVARCHAR (5)   NOT NULL,
    [CreatedDate]       DATETIME       NOT NULL,
    [CreatedBy]         NVARCHAR (325) NOT NULL,
    CONSTRAINT [PK_LogicalOperator] PRIMARY KEY CLUSTERED ([LogicalOperatorId] ASC)
);


GO

-- =============================================
-- Author:		Monte Jennings
-- Create date: 5/22/2019 1:41:42 PM
-- Description: Inserts old records edited or deleted from the LogicalOperator table.
-- =============================================
CREATE TRIGGER [CustomField].LogicalOperatorAuditTrigger 
   ON  [CustomField].[LogicalOperator] 
   AFTER DELETE,UPDATE
AS 
BEGIN
SET NOCOUNT ON;

    DECLARE @Count as int

    SELECT @Count = COUNT(*) FROM DELETED

   IF @Count > 0
		 BEGIN
			INSERT INTO Audit.[LogicalOperator](
				 [LogicalOperatorId]
				,[Name]
				,[CreatedDate]
				,[CreatedBy])
			SELECT 
				 [LogicalOperatorId]
				,[Name]
				,[CreatedDate]
				,[CreatedBy]			
			FROM DELETED
		END
	ELSE
		BEGIN
			INSERT INTO Audit.[LogicalOperator](
				 [LogicalOperatorId]
				,[Name]
				,[CreatedDate]
				,[CreatedBy])
			SELECT 
				 [LogicalOperatorId]
				,[Name]
				,[CreatedDate]
				,[CreatedBy]		
			FROM INSERTED
		END	
       
END


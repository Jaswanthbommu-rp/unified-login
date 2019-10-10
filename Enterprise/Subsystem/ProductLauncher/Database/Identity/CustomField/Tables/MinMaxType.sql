CREATE TABLE [CustomField].[MinMaxType] (
    [MinMaxTypeId]   TINYINT        IDENTITY (1, 1) NOT NULL,
    [MinMaxTypeName] NVARCHAR (50)  NOT NULL,
    [CreatedDate]    DATETIME       CONSTRAINT [DF_MinMaxType_CreatedDate] DEFAULT (getdate()) NOT NULL,
    [CreatedBy]      NVARCHAR (325) CONSTRAINT [DF_MinMaxType_CreatedBy] DEFAULT ('00000000-0000-0000-0000-000000000000') NOT NULL,
    CONSTRAINT [PK_MinMaxType] PRIMARY KEY CLUSTERED ([MinMaxTypeId] ASC)
);


GO

-- =============================================
-- Author:		Monte Jennings
-- Create date: 5/22/2019 1:41:43 PM
-- Description: Inserts old records edited or deleted from the MinMaxType table.
-- =============================================
CREATE TRIGGER [CustomField].MinMaxTypeAuditTrigger 
   ON  [CustomField].[MinMaxType] 
   AFTER DELETE,UPDATE
AS 
BEGIN
SET NOCOUNT ON;

    DECLARE @Count as int

    SELECT @Count = COUNT(*) FROM DELETED

   IF @Count > 0
		 BEGIN
			INSERT INTO Audit.[MinMaxType](
				 [MinMaxTypeId]
				,[MinMaxTypeName])
			SELECT 
				 [MinMaxTypeId]
				,[MinMaxTypeName]			
			FROM DELETED
		END
	ELSE
		BEGIN
			INSERT INTO Audit.[MinMaxType](
				 [MinMaxTypeId]
				,[MinMaxTypeName])
			SELECT 
				 [MinMaxTypeId]
				,[MinMaxTypeName]		
			FROM INSERTED
		END	
       
END


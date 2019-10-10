CREATE TABLE [CustomField].[Option] (
    [OptionId]    BIGINT          IDENTITY (1, 1) NOT NULL,
    [FieldId]     BIGINT          NOT NULL,
    [Name]        NVARCHAR (1024) NOT NULL,
    [CreatedDate] DATETIME        NOT NULL,
    [CreatedBy]   NVARCHAR (325)  NOT NULL,
    CONSTRAINT [PK_Option] PRIMARY KEY CLUSTERED ([OptionId] ASC),
    CONSTRAINT [FK_Option_Field] FOREIGN KEY ([FieldId]) REFERENCES [CustomField].[Field] ([FieldId])
);


GO

-- =============================================
-- Author:		Monte Jennings
-- Create date: 5/22/2019 1:41:43 PM
-- Description: Inserts old records edited or deleted from the Option table.
-- =============================================
CREATE TRIGGER [CustomField].OptionAuditTrigger 
   ON  [CustomField].[Option] 
   AFTER DELETE,UPDATE
AS 
BEGIN
SET NOCOUNT ON;

    DECLARE @Count as int

    SELECT @Count = COUNT(*) FROM DELETED

   IF @Count > 0
		 BEGIN
			INSERT INTO Audit.[Option](
				 [OptionId]
				,[FieldId]
				,[Name]
				,[CreatedDate]
				,[CreatedBy])
			SELECT 
				 [OptionId]
				,[FieldId]
				,[Name]
				,[CreatedDate]
				,[CreatedBy]			
			FROM DELETED
		END
	ELSE
		BEGIN
			INSERT INTO Audit.[Option](
				 [OptionId]
				,[FieldId]
				,[Name]
				,[CreatedDate]
				,[CreatedBy])
			SELECT 
				 [OptionId]
				,[FieldId]
				,[Name]
				,[CreatedDate]
				,[CreatedBy]		
			FROM INSERTED
		END	
       
END


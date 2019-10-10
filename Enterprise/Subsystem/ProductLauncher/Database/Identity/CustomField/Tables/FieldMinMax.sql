CREATE TABLE [CustomField].[FieldMinMax] (
    [FieldId]      BIGINT         NOT NULL,
    [MinMaxTypeId] TINYINT        NOT NULL,
    [Minimum]      INT            NOT NULL,
    [Maximum]      INT            NOT NULL,
    [CreatedBy]    NVARCHAR (325) NOT NULL,
    [CreatedDate]  DATETIME       NOT NULL,
    CONSTRAINT [PK_CustomFieldMinMax_1] PRIMARY KEY CLUSTERED ([FieldId] ASC),
    CONSTRAINT [FK_CustomFieldMinMax_CustomField] FOREIGN KEY ([FieldId]) REFERENCES [CustomField].[Field] ([FieldId]),
    CONSTRAINT [FK_CustomFieldMinMax_MinMaxType] FOREIGN KEY ([MinMaxTypeId]) REFERENCES [CustomField].[MinMaxType] ([MinMaxTypeId])
);


GO

-- =============================================
-- Author:		Monte Jennings
-- Create date: 5/22/2019 1:41:28 PM
-- Description: Inserts old records edited or deleted from the FieldMinMax table.
-- =============================================
CREATE TRIGGER [CustomField].FieldMinMaxAuditTrigger 
   ON  [CustomField].[FieldMinMax] 
   AFTER DELETE,UPDATE
AS 
BEGIN
SET NOCOUNT ON;

    DECLARE @Count as int

    SELECT @Count = COUNT(*) FROM DELETED

   IF @Count > 0
		 BEGIN
			INSERT INTO Audit.[FieldMinMax](
				 [FieldId]
				,[MinMaxTypeId]
				,[Minimum]
				,[Maximum]
				,[CreatedBy]
				,[CreatedDate])
			SELECT 
				 [FieldId]
				,[MinMaxTypeId]
				,[Minimum]
				,[Maximum]
				,[CreatedBy]
				,[CreatedDate]			
			FROM DELETED
		END
	ELSE
		BEGIN
			INSERT INTO Audit.[FieldMinMax](
				 [FieldId]
				,[MinMaxTypeId]
				,[Minimum]
				,[Maximum]
				,[CreatedBy]
				,[CreatedDate])
			SELECT 
				 [FieldId]
				,[MinMaxTypeId]
				,[Minimum]
				,[Maximum]
				,[CreatedBy]
				,[CreatedDate]		
			FROM INSERTED
		END	
       
END


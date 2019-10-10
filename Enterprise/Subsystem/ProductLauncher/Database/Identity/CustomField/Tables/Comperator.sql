CREATE TABLE [CustomField].[Comperator] (
    [ComperatorId] TINYINT        IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (50)  NOT NULL,
    [CreatedDate]  DATETIME       NOT NULL,
    [CreatedBy]    NVARCHAR (325) NULL,
    CONSTRAINT [PK_Comperator] PRIMARY KEY CLUSTERED ([ComperatorId] ASC)
);


GO

-- =============================================
-- Author:		Monte Jennings
-- Create date: 5/22/2019 1:39:57 PM
-- Description: Inserts old records edited or deleted from the Comperator table.
-- =============================================
CREATE TRIGGER [CustomField].ComperatorAuditTrigger 
   ON  [CustomField].[Comperator] 
   AFTER DELETE,UPDATE
AS 
BEGIN
SET NOCOUNT ON;

    DECLARE @Count as int

    SELECT @Count = COUNT(*) FROM DELETED

   IF @Count > 0
		 BEGIN
			INSERT INTO Audit.[Comperator](
				 [ComperatorId]
				,[Name]
				,[CreatedDate]
				,[CreatedBy])
			SELECT 
				 [ComperatorId]
				,[Name]
				,[CreatedDate]
				,[CreatedBy]			
			FROM DELETED
		END
	ELSE
		BEGIN
			INSERT INTO Audit.[Comperator](
				 [ComperatorId]
				,[Name]
				,[CreatedDate]
				,[CreatedBy])
			SELECT 
				 [ComperatorId]
				,[Name]
				,[CreatedDate]
				,[CreatedBy]		
			FROM INSERTED
		END	
       
END


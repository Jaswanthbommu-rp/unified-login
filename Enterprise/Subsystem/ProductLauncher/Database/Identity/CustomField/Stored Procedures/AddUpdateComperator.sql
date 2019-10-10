
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the Comperator table.
-- =============================================
CREATE PROCEDURE [CustomField].AddUpdateComperator (
	 @ComperatorId TINYINT
	,@Name NVARCHAR(100)
	,@CreatedDate DATETIME
	,@CreatedBy NVARCHAR(650)
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [CustomField].[Comperator] WHERE [ComperatorId] = @ComperatorId) 
	BEGIN
		UPDATE [CustomField].[Comperator] SET 
			 [Name] = @Name
			,[CreatedDate] = @CreatedDate
			,[CreatedBy] = @CreatedBy
		WHERE
			[ComperatorId] = @ComperatorId

		SELECT			
			 [ComperatorId]
			,[Name]
			,[CreatedDate]
			,[CreatedBy]
		FROM
			[CustomField].[Comperator]
		WHERE
			[ComperatorId] = @ComperatorId
	END
	ELSE
	BEGIN
		INSERT INTO [CustomField].[Comperator] (
			 [Name]
			,[CreatedDate]
			,[CreatedBy])
		OUTPUT 
			 inserted.[ComperatorId]
			,inserted.[Name]
			,inserted.[CreatedDate]
			,inserted.[CreatedBy]
		VALUES(
			 @Name
			,@CreatedDate
			,@CreatedBy);

	

	END;

-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the Comparator table.
-- =============================================
CREATE PROCEDURE [Enterprise].AddUpdateComparator (
	 @ComparatorId INT
	,@Name NVARCHAR(100)
	,@CreatedDate DATETIME
	,@CreatedBy BIGINT
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [Enterprise].[Comparator] WHERE [ComparatorId] = @ComparatorId) 
	BEGIN
		UPDATE [Enterprise].[Comparator] SET 
			 [Name] = @Name
			,[CreatedDate] = @CreatedDate
			,[CreatedBy] = @CreatedBy
		WHERE
			[ComparatorId] = @ComparatorId

		SELECT			
			 [ComparatorId]
			,[Name]
			,[CreatedDate]
			,[CreatedBy]
		FROM
			[Enterprise].[Comparator]
		WHERE
			[ComparatorId] = @ComparatorId
	END
	ELSE
	BEGIN
		INSERT INTO [Enterprise].[Comparator] (
			 [Name]
			,[CreatedDate]
			,[CreatedBy])
		OUTPUT 
			 inserted.[ComparatorId]
			,inserted.[Name]
			,inserted.[CreatedDate]
			,inserted.[CreatedBy]
		VALUES(
			 @Name
			,@CreatedDate
			,@CreatedBy);

	

	END;
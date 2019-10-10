
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the LogicalOperator table.
-- =============================================
CREATE PROCEDURE [CustomField].AddUpdateLogicalOperator (
	 @LogicalOperatorId TINYINT
	,@Name NVARCHAR(10)
	,@CreatedDate DATETIME
	,@CreatedBy NVARCHAR(650)
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [CustomField].[LogicalOperator] WHERE [LogicalOperatorId] = @LogicalOperatorId) 
	BEGIN
		UPDATE [CustomField].[LogicalOperator] SET 
			 [Name] = @Name
			,[CreatedDate] = @CreatedDate
			,[CreatedBy] = @CreatedBy
		WHERE
			[LogicalOperatorId] = @LogicalOperatorId

		SELECT			
			 [LogicalOperatorId]
			,[Name]
			,[CreatedDate]
			,[CreatedBy]
		FROM
			[CustomField].[LogicalOperator]
		WHERE
			[LogicalOperatorId] = @LogicalOperatorId
	END
	ELSE
	BEGIN
		INSERT INTO [CustomField].[LogicalOperator] (
			 [Name]
			,[CreatedDate]
			,[CreatedBy])
		OUTPUT 
			 inserted.[LogicalOperatorId]
			,inserted.[Name]
			,inserted.[CreatedDate]
			,inserted.[CreatedBy]
		VALUES(
			 @Name
			,@CreatedDate
			,@CreatedBy);

	

	END;
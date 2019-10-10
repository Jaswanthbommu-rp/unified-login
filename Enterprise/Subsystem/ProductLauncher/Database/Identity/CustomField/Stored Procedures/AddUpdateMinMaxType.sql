
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the MinMaxType table.
-- =============================================
CREATE PROCEDURE [CustomField].AddUpdateMinMaxType (
	 @MinMaxTypeId TINYINT
	,@MinMaxTypeName NVARCHAR(100)
	,@CreatedDate DATETIME
	,@CreatedBy NVARCHAR(650)
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [CustomField].[MinMaxType] WHERE [MinMaxTypeId] = @MinMaxTypeId) 
	BEGIN
		UPDATE [CustomField].[MinMaxType] SET 
			 [MinMaxTypeName] = @MinMaxTypeName
			,[CreatedDate] = @CreatedDate
			,[CreatedBy] = @CreatedBy
		WHERE
			[MinMaxTypeId] = @MinMaxTypeId

		SELECT			
			 [MinMaxTypeId]
			,[MinMaxTypeName]
			,[CreatedDate]
			,[CreatedBy]
		FROM
			[CustomField].[MinMaxType]
		WHERE
			[MinMaxTypeId] = @MinMaxTypeId
	END
	ELSE
	BEGIN
		INSERT INTO [CustomField].[MinMaxType] (
			 [MinMaxTypeName]
			,[CreatedDate]
			,[CreatedBy])
		OUTPUT 
			 inserted.[MinMaxTypeId]
			,inserted.[MinMaxTypeName]
			,inserted.[CreatedDate]
			,inserted.[CreatedBy]
		VALUES(
			 @MinMaxTypeName
			,@CreatedDate
			,@CreatedBy);

	

	END;
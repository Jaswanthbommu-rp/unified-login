
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the FieldType table.
-- =============================================
CREATE PROCEDURE [CustomField].[AddUpdateFieldType] (
	 @FieldTypeId TINYINT
	,@Name NVARCHAR(100)
	,@Description NVARCHAR(500)
	,@CreatedDate DATETIME
	,@CreatedBy BIGINT
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [CustomField].[FieldType] WHERE [FieldTypeId] = @FieldTypeId) 
	BEGIN
		UPDATE [CustomField].[FieldType] SET 
			 [Name] = @Name
			,[Description] = @Description
			,[CreatedDate] = @CreatedDate
			,[CreatedBy] = @CreatedBy
		WHERE
			[FieldTypeId] = @FieldTypeId

		SELECT			
			 [FieldTypeId]
			,[Name]
			,[Description]
			,[CreatedDate]
			,[CreatedBy]
		FROM
			[CustomField].[FieldType]
		WHERE
			[FieldTypeId] = @FieldTypeId
	END
	ELSE
	BEGIN
		INSERT INTO [CustomField].[FieldType] (
			 [Name]
			,[Description]
			,[CreatedDate]
			,[CreatedBy])
		OUTPUT 
			 inserted.[FieldTypeId]
			,inserted.[Name]
			,inserted.[Description]
			,inserted.[CreatedDate]
			,inserted.[CreatedBy]
		VALUES(
			 @Name
			,@Description
			,@CreatedDate
			,@CreatedBy);

	

	END;

-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the Option table.
-- =============================================
CREATE PROCEDURE [CustomField].AddUpdateOption (
	 @OptionId BIGINT
	,@FieldId BIGINT
	,@Name NVARCHAR(2048)
	,@CreatedDate DATETIME
	,@CreatedBy NVARCHAR(650)
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [CustomField].[Option] WHERE [OptionId] = @OptionId) 
	BEGIN
		UPDATE [CustomField].[Option] SET 
			 [FieldId] = @FieldId
			,[Name] = @Name
			,[CreatedDate] = @CreatedDate
			,[CreatedBy] = @CreatedBy
		WHERE
			[OptionId] = @OptionId

		SELECT			
			 [OptionId]
			,[FieldId]
			,[Name]
			,[CreatedDate]
			,[CreatedBy]
		FROM
			[CustomField].[Option]
		WHERE
			[OptionId] = @OptionId
	END
	ELSE
	BEGIN
		INSERT INTO [CustomField].[Option] (
			 [FieldId]
			,[Name]
			,[CreatedDate]
			,[CreatedBy])
		OUTPUT 
			 inserted.[OptionId]
			,inserted.[FieldId]
			,inserted.[Name]
			,inserted.[CreatedDate]
			,inserted.[CreatedBy]
		VALUES(
			 @FieldId
			,@Name
			,@CreatedDate
			,@CreatedBy);

	

	END;
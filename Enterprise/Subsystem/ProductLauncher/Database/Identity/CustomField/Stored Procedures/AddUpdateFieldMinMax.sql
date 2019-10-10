
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the FieldMinMax table.
-- =============================================
CREATE PROCEDURE [CustomField].AddUpdateFieldMinMax (
	 @FieldId BIGINT
	,@MinMaxTypeId TINYINT
	,@Minimum INT
	,@Maximum INT
	,@CreatedBy NVARCHAR(650)
	,@CreatedDate DATETIME
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [CustomField].[FieldMinMax] WHERE [FieldId] = @FieldId) 
	BEGIN
		UPDATE [CustomField].[FieldMinMax] SET 
			 [MinMaxTypeId] = @MinMaxTypeId
			,[Minimum] = @Minimum
			,[Maximum] = @Maximum
			,[CreatedBy] = @CreatedBy
			,[CreatedDate] = @CreatedDate
		WHERE
			[FieldId] = @FieldId

		SELECT			
			 [FieldId]
			,[MinMaxTypeId]
			,[Minimum]
			,[Maximum]
			,[CreatedBy]
			,[CreatedDate]
		FROM
			[CustomField].[FieldMinMax]
		WHERE
			[FieldId] = @FieldId
	END
	ELSE
	BEGIN
		INSERT INTO [CustomField].[FieldMinMax] (
			 [MinMaxTypeId]
			,[Minimum]
			,[Maximum]
			,[CreatedBy]
			,[CreatedDate])
		OUTPUT 
			 inserted.[FieldId]
			,inserted.[MinMaxTypeId]
			,inserted.[Minimum]
			,inserted.[Maximum]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
		VALUES(
			 @MinMaxTypeId
			,@Minimum
			,@Maximum
			,@CreatedBy
			,@CreatedDate);

	

	END;
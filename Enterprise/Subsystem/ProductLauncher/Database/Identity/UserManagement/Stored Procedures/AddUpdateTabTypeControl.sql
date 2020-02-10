
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the TabTypeControl table.
-- =============================================
CREATE PROCEDURE [UserManagement].AddUpdateTabTypeControl (
	 @TabTypeControlId INT
	,@TabTypeId INT
	,@ProductPageId INT
	,@ControlId INT
	,@CreatedBy BIGINT
	,@CreatedDate DATETIME
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [UserManagement].[TabTypeControl] WHERE [TabTypeControlId] = @TabTypeControlId) 
	BEGIN
		UPDATE [UserManagement].[TabTypeControl] SET 
			 [TabTypeId] = @TabTypeId
			,[ProductPageId] = @ProductPageId
			,[ControlId] = @ControlId
			,[CreatedBy] = @CreatedBy
			,[CreatedDate] = @CreatedDate
		WHERE
			[TabTypeControlId] = @TabTypeControlId

		SELECT			
			 [TabTypeControlId]
			,[TabTypeId]
			,[ProductPageId]
			,[ControlId]
			,[CreatedBy]
			,[CreatedDate]
		FROM
			[UserManagement].[TabTypeControl]
		WHERE
			[TabTypeControlId] = @TabTypeControlId
	END
	ELSE
	BEGIN
		INSERT INTO [UserManagement].[TabTypeControl] (
			 [TabTypeId]
			,[ProductPageId]
			,[ControlId]
			,[CreatedBy]
			,[CreatedDate])
		OUTPUT 
			 inserted.[TabTypeControlId]
			,inserted.[TabTypeId]
			,inserted.[ProductPageId]
			,inserted.[ControlId]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
		VALUES(
			 @TabTypeId
			,@ProductPageId
			,@ControlId
			,@CreatedBy
			,@CreatedDate);

	

	END;
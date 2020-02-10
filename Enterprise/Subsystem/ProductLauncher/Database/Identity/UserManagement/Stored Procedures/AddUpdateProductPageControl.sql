
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the ProductPageControl table.
-- =============================================
CREATE PROCEDURE [UserManagement].AddUpdateProductPageControl (
	 @ProductPageControlId INT
	,@ProductPageId INT
	,@ControlId INT
	,@CreatedBy BIGINT
	,@CreatedDate DATETIME
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [UserManagement].[ProductPageControl] WHERE [ProductPageControlId] = @ProductPageControlId) 
	BEGIN
		UPDATE [UserManagement].[ProductPageControl] SET 
			 [ProductPageId] = @ProductPageId
			,[ControlId] = @ControlId
			,[CreatedBy] = @CreatedBy
			,[CreatedDate] = @CreatedDate
		WHERE
			[ProductPageControlId] = @ProductPageControlId

		SELECT			
			 [ProductPageControlId]
			,[ProductPageId]
			,[ControlId]
			,[CreatedBy]
			,[CreatedDate]
		FROM
			[UserManagement].[ProductPageControl]
		WHERE
			[ProductPageControlId] = @ProductPageControlId
	END
	ELSE
	BEGIN
		INSERT INTO [UserManagement].[ProductPageControl] (
			 [ProductPageId]
			,[ControlId]
			,[CreatedBy]
			,[CreatedDate])
		OUTPUT 
			 inserted.[ProductPageControlId]
			,inserted.[ProductPageId]
			,inserted.[ControlId]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
		VALUES(
			 @ProductPageId
			,@ControlId
			,@CreatedBy
			,@CreatedDate);

	

	END;
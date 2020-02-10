
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the ProductPage table.
-- =============================================
CREATE PROCEDURE [UserManagement].AddUpdateProductPage (
	 @ProductPageId INT
	,@ProductId INT
	,@DisplayName NVARCHAR(510)
	,@CreatedBy BIGINT
	,@CreatedDate DATETIME
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [UserManagement].[ProductPage] WHERE [ProductPageId] = @ProductPageId) 
	BEGIN
		UPDATE [UserManagement].[ProductPage] SET 
			 [ProductId] = @ProductId
			,[DisplayName] = @DisplayName
			,[CreatedBy] = @CreatedBy
			,[CreatedDate] = @CreatedDate
		WHERE
			[ProductPageId] = @ProductPageId

		SELECT			
			 [ProductPageId]
			,[ProductId]
			,[DisplayName]
			,[CreatedBy]
			,[CreatedDate]
		FROM
			[UserManagement].[ProductPage]
		WHERE
			[ProductPageId] = @ProductPageId
	END
	ELSE
	BEGIN
		INSERT INTO [UserManagement].[ProductPage] (
			 [ProductId]
			,[DisplayName]
			,[CreatedBy]
			,[CreatedDate])
		OUTPUT 
			 inserted.[ProductPageId]
			,inserted.[ProductId]
			,inserted.[DisplayName]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
		VALUES(
			 @ProductId
			,@DisplayName
			,@CreatedBy
			,@CreatedDate);

	

	END;
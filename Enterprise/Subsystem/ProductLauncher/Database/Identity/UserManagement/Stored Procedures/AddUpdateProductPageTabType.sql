
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the ProductPageTabType table.
-- =============================================
CREATE PROCEDURE [UserManagement].AddUpdateProductPageTabType (
	 @ProductPageTabTypeId INT
	,@ProductPageId INT
	,@TabTypeId INT
	,@Sequence TINYINT
	,@CreatedBy BIGINT
	,@CreatedDate DATETIME
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [UserManagement].[ProductPageTabType] WHERE [ProductPageTabTypeId] = @ProductPageTabTypeId) 
	BEGIN
		UPDATE [UserManagement].[ProductPageTabType] SET 
			 [ProductPageId] = @ProductPageId
			,[TabTypeId] = @TabTypeId
			,[Sequence] = @Sequence
			,[CreatedBy] = @CreatedBy
			,[CreatedDate] = @CreatedDate
		WHERE
			[ProductPageTabTypeId] = @ProductPageTabTypeId

		SELECT			
			 [ProductPageTabTypeId]
			,[ProductPageId]
			,[TabTypeId]
			,[Sequence]
			,[CreatedBy]
			,[CreatedDate]
		FROM
			[UserManagement].[ProductPageTabType]
		WHERE
			[ProductPageTabTypeId] = @ProductPageTabTypeId
	END
	ELSE
	BEGIN
		INSERT INTO [UserManagement].[ProductPageTabType] (
			 [ProductPageId]
			,[TabTypeId]
			,[Sequence]
			,[CreatedBy]
			,[CreatedDate])
		OUTPUT 
			 inserted.[ProductPageTabTypeId]
			,inserted.[ProductPageId]
			,inserted.[TabTypeId]
			,inserted.[Sequence]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
		VALUES(
			 @ProductPageId
			,@TabTypeId
			,@Sequence
			,@CreatedBy
			,@CreatedDate);

	

	END;
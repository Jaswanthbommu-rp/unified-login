
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the TabType table.
-- =============================================
CREATE PROCEDURE [UserManagement].AddUpdateTabType (
	 @TabTypeId INT
	,@UIId NVARCHAR(510)
	,@DisplayName NVARCHAR(510)
	,@CreatedBy BIGINT
	,@CreatedDate DATETIME
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [UserManagement].[TabType] WHERE [TabTypeId] = @TabTypeId) 
	BEGIN
		UPDATE [UserManagement].[TabType] SET 
			 [UIId] = @UIId
			,[DisplayName] = @DisplayName
			,[CreatedBy] = @CreatedBy
			,[CreatedDate] = @CreatedDate
		WHERE
			[TabTypeId] = @TabTypeId

		SELECT			
			 [TabTypeId]
			,[UIId]
			,[DisplayName]
			,[CreatedBy]
			,[CreatedDate]
		FROM
			[UserManagement].[TabType]
		WHERE
			[TabTypeId] = @TabTypeId
	END
	ELSE
	BEGIN
		INSERT INTO [UserManagement].[TabType] (
			 [UIId]
			,[DisplayName]
			,[CreatedBy]
			,[CreatedDate])
		OUTPUT 
			 inserted.[TabTypeId]
			,inserted.[UIId]
			,inserted.[DisplayName]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
		VALUES(
			 @UIId
			,@DisplayName
			,@CreatedBy
			,@CreatedDate);

	

	END;
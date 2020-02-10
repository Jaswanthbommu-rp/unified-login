
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the TabType table.
-- =============================================
CREATE PROCEDURE [UserManagement].AddUpdateTabType (
	 @TabTypeId INT
	,@UIId NVARCHAR(510)
	,@DisplayName NVARCHAR(510)
	,@Sequence TINYINT
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
			,[Sequence] = @Sequence
			,[CreatedBy] = @CreatedBy
			,[CreatedDate] = @CreatedDate
		WHERE
			[TabTypeId] = @TabTypeId

		SELECT			
			 [TabTypeId]
			,[UIId]
			,[DisplayName]
			,[Sequence]
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
			,[Sequence]
			,[CreatedBy]
			,[CreatedDate])
		OUTPUT 
			 inserted.[TabTypeId]
			,inserted.[UIId]
			,inserted.[DisplayName]
			,inserted.[Sequence]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
		VALUES(
			 @UIId
			,@DisplayName
			,@Sequence
			,@CreatedBy
			,@CreatedDate);

	

	END;
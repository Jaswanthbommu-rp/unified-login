
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the Control table.
-- =============================================
CREATE PROCEDURE [UserManagement].AddUpdateControl (
	 @ControlId INT
	,@ParentControlId INT
	,@ControlTypeId INT
	,@UIId NVARCHAR(510)
	,@DisplayName NVARCHAR(510)
	,@DataSource NTEXT
	,@Sequence TINYINT
	,@CreatedBy BIGINT
	,@CreatedDate DATETIME
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [UserManagement].[Control] WHERE [ControlId] = @ControlId) 
	BEGIN
		UPDATE [UserManagement].[Control] SET 
			 [ParentControlId] = @ParentControlId
			,[ControlTypeId] = @ControlTypeId
			,[UIId] = @UIId
			,[DisplayName] = @DisplayName
			,[DataSource] = @DataSource
			,[Sequence] = @Sequence
			,[CreatedBy] = @CreatedBy
			,[CreatedDate] = @CreatedDate
		WHERE
			[ControlId] = @ControlId

		SELECT			
			 [ControlId]
			,[ParentControlId]
			,[ControlTypeId]
			,[UIId]
			,[DisplayName]
			,[DataSource]
			,[Sequence]
			,[CreatedBy]
			,[CreatedDate]
		FROM
			[UserManagement].[Control]
		WHERE
			[ControlId] = @ControlId
	END
	ELSE
	BEGIN
		INSERT INTO [UserManagement].[Control] (
			 [ParentControlId]
			,[ControlTypeId]
			,[UIId]
			,[DisplayName]
			,[DataSource]
			,[Sequence]
			,[CreatedBy]
			,[CreatedDate])
		OUTPUT 
			 inserted.[ControlId]
			,inserted.[ParentControlId]
			,inserted.[ControlTypeId]
			,inserted.[UIId]
			,inserted.[DisplayName]
			,inserted.[DataSource]
			,inserted.[Sequence]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
		VALUES(
			 @ParentControlId
			,@ControlTypeId
			,@UIId
			,@DisplayName
			,@DataSource
			,@Sequence
			,@CreatedBy
			,@CreatedDate);

	

	END;
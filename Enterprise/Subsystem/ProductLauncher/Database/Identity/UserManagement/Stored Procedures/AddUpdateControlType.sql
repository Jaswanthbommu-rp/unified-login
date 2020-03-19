
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the ControlType table.
-- =============================================
CREATE PROCEDURE [UserManagement].[AddUpdateControlType] (
	 @ControlTypeId INT
	,@Name NVARCHAR(100)
	,@Description NVARCHAR(510)
	,@CreatedBy BIGINT
	,@CreatedDate DATETIME
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [UserManagement].[ControlType] WHERE [ControlTypeId] = @ControlTypeId) 
	BEGIN
		UPDATE [UserManagement].[ControlType] SET 
			 [Name] = @Name
			,[Description] = @Description
			,[CreatedBy] = @CreatedBy
			,[CreatedDate] = @CreatedDate
		WHERE
			[ControlTypeId] = @ControlTypeId

		SELECT			
			 [ControlTypeId]
			,[Name]
			,[Description]
			,[CreatedBy]
			,[CreatedDate]
		FROM
			[UserManagement].[ControlType]
		WHERE
			[ControlTypeId] = @ControlTypeId
	END
	ELSE
	BEGIN
		INSERT INTO [UserManagement].[ControlType] (
			 [Name]
			,[Description]
			,[CreatedBy]
			,[CreatedDate])
		OUTPUT 
			 inserted.[ControlTypeId]
			,inserted.[Name]
			,inserted.[Description]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
		VALUES(
			 @Name
			,@Description
			,@CreatedBy
			,@CreatedDate);

	

	END;
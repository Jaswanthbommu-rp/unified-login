
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Adds new/Updates the ControlAttribute table.
-- =============================================
CREATE PROCEDURE [UserManagement].[AddUpdateControlAttribute] (
	 @ControlAttributeId INT
	,@ControlId INT
	,@Key NVARCHAR(100)
	,@Value NVARCHAR(100)
	,@CreatedBy BIGINT
	,@CreatedDate DATETIME
) 

 AS 
	SET NOCOUNT ON 

	IF EXISTS(SELECT 1 FROM [UserManagement].[ControlAttribute] WHERE [ControlAttributeId] = @ControlAttributeId) 
	BEGIN
		UPDATE [UserManagement].[ControlAttribute] SET 
			 [ControlId] = @ControlId
			,[Key] = @Key
			,[Value] = @Value
			,[CreatedBy] = @CreatedBy
			,[CreatedDate] = @CreatedDate
		WHERE
			[ControlAttributeId] = @ControlAttributeId

		SELECT			
			 [ControlAttributeId]
			,[ControlId]
			,[Key]
			,[Value]
			,[CreatedBy]
			,[CreatedDate]
		FROM
			[UserManagement].[ControlAttribute]
		WHERE
			[ControlAttributeId] = @ControlAttributeId
	END
	ELSE
	BEGIN
		INSERT INTO [UserManagement].[ControlAttribute] (
			 [ControlId]
			,[Key]
			,[Value]
			,[CreatedBy]
			,[CreatedDate])
		OUTPUT 
			 inserted.[ControlAttributeId]
			,inserted.[ControlId]
			,inserted.[Key]
			,inserted.[Value]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
		VALUES(
			 @ControlId
			,@Key
			,@Value
			,@CreatedBy
			,@CreatedDate);

	

	END;
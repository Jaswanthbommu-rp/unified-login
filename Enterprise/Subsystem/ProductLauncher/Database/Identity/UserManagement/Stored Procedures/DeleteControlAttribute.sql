
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the ControlAttribute table.
-- =============================================
CREATE PROCEDURE [UserManagement].DeleteControlAttribute (
	 @ControlAttributeId INT = NULL 
	,@ControlId INT = NULL 
	,@Key NVARCHAR(100) = NULL 
	,@Value NVARCHAR(100) = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS
BEGIN



	DELETE 
		[UserManagement].[ControlAttribute]

	WHERE
		(@ControlAttributeId IS NULL  OR  [ControlAttributeId] = @ControlAttributeId)
	AND
		(@ControlId IS NULL  OR  [ControlId] = @ControlId)
	AND
		(@Key IS NULL  OR  [Key] = @Key)
	AND
		(@Value IS NULL  OR  [Value] = @Value)

	RETURN 1 --for success

END
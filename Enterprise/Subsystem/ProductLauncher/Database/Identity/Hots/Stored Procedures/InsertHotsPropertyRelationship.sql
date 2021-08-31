CREATE PROCEDURE [Hots].[InsertHotsPropertyRelationship]
	@BaseLineProperty UNIQUEIDENTIFIER,
	@CloneProperty UNIQUEIDENTIFIER,
	@UserId INT = 1
AS
BEGIN
	DECLARE @BaseLinePropertyInstanceId BIGINT, 
			@ClonePropertyInstanceId BIGINT

	SELECT @BaseLinePropertyInstanceId = PropertyInstanceId From Enterprise.PropertyInstance WHERE InstanceId = @BaseLineProperty
	SELECT @ClonePropertyInstanceId = PropertyInstanceId From Enterprise.PropertyInstance WHERE InstanceId = @CloneProperty

	IF @BaseLinePropertyInstanceId IS NULL OR @ClonePropertyInstanceId IS NULL
	BEGIN
		SELECT 0 [Id], 'Failed to locate ' + CASE WHEN @BaseLinePropertyInstanceId IS NULL THEN 'BaseLineProperty: '+CONVERT(VARCHAR(40),@BaseLineProperty) ELSE 'CloneProperty: '+CONVERT(VARCHAR(40),@CloneProperty) END [ErrorMessage]
		RETURN 0
	END

	IF @BaseLineProperty = @CloneProperty OR @BaseLinePropertyInstanceId = @ClonePropertyInstanceId
	BEGIN
		SELECT 0 AS Id, 'Property ids cannot be for the same property.' [ErrorMessage]
		RETURN 0
	END

	INSERT INTO Hots.PropertyRelationship ( BasePropertyInstanceId, ClonePropertyInstanceId, CreateDate, CreatedBy )
	VALUES
		( @BaseLinePropertyInstanceId, @ClonePropertyInstanceId, GETUTCDATE(), @UserId )
	
	SELECT SCOPE_IDENTITY() [Id], '' [ErrorMessage]
END

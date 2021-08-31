CREATE PROCEDURE [Hots].[InsertHotsPropertyRelationship]
	@BaseLineProperty UNIQUEIDENTIFIER,
	@CloneProperty UNIQUEIDENTIFIER,
	@CloneCompany UNIQUEIDENTIFIER,
	@UserId INT = 1
AS
BEGIN
	DECLARE @BaseLinePropertyInstanceId BIGINT, 
			@ClonePropertyInstanceId BIGINT,
			@CloneCompanyPartyId BIGINT

	SELECT @BaseLinePropertyInstanceId = PropertyInstanceId From Enterprise.PropertyInstance WHERE InstanceId = @BaseLineProperty
	SELECT @ClonePropertyInstanceId = PropertyInstanceId From Enterprise.PropertyInstance WHERE InstanceId = @CloneProperty
	SELECT @CloneCompanyPartyId = PartyId From Enterprise.Party WHERE RealPageId = @CloneCompany

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

	INSERT INTO Hots.PropertyRelationship ( BasePropertyInstanceId, ClonePropertyInstanceId, CloneCompanyPartyId, CreateDate, CreatedBy )
	VALUES
		( @BaseLinePropertyInstanceId, @ClonePropertyInstanceId, @CloneCompanyPartyId, GETUTCDATE(), @UserId )
	
	SELECT SCOPE_IDENTITY() [Id], '' [ErrorMessage]
END

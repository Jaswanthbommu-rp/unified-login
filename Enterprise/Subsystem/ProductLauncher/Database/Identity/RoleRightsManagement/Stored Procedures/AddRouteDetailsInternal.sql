CREATE PROCEDURE [Security].[AddRouteDetailsInternal]
	@RouteValue nvarchar(50),
	@Description nvarchar(200),
	@CreatedBy nvarchar(255)
AS
BEGIN
	DECLARE @RouteId INT
	INSERT INTO [Security].[Route]
		(RouteValue,
		[Description],
		CreatedBy,
		CreatedDate
		)
	VALUES
		(@RouteValue,
		@Description,
		@CreatedBy,
		GETUTCDATE()
		);
	SELECT @RouteId = SCOPE_IDENTITY();
	SELECT @RouteId AS Id, '' AS ErrorMessage
END;
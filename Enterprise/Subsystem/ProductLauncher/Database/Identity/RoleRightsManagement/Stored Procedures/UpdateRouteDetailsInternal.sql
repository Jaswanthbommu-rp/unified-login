CREATE PROCEDURE [Security].[UpdateRouteDetailsInternal]
    @RouteId int,
	@RouteValue nvarchar(50),
	@Description nvarchar(200)
AS
BEGIN
	UPDATE [Security].[Route]
	SET [Description] = @Description, RouteValue = @RouteValue
	WHERE RouteId = @RouteId
	SELECT @RouteId AS RouteId
END;
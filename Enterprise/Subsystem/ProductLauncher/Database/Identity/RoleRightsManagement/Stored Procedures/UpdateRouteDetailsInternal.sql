CREATE PROCEDURE [Security].[UpdateRouteDetailsInternal]
    @RouteId int,
	@Description nvarchar(200)
AS
BEGIN
	UPDATE [Security].[Route]
	SET [Description] = @Description
	WHERE RouteId = @RouteId
	SELECT @RouteId AS RouteId
END;
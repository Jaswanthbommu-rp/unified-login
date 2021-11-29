CREATE PROCEDURE [Security].[GetRoutesForRight](
    @RightId int
)
AS
    BEGIN
		SELECT R.RouteId,
			   R.RouteValue,
			   R.Description,
			   R.CreatedBy,
			   R.CreatedDate
		FROM [Security].RightRoute RR
		INNER JOIN [Security].[Route] R ON R.RouteId = RR.RouteId
		WHERE RR.RightId = @RightId
	END

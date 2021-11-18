CREATE PROCEDURE [Security].[UpdateRightsForRoute](
    @RouteId int,
	@RightIdsList nvarchar(max),
	@CreatedBy INT
)
AS
BEGIN
DECLARE @CreatedDate datetime = GETUTCDATE()
DECLARE @Rights TABLE (  
	RightId int PRIMARY KEY  
)  
IF (LEN(@RightIdsList) > 0)  
 BEGIN  
  INSERT INTO @Rights (  
   RightId  
  )  
  SELECT CONVERT(int, value)  
  FROM STRING_SPLIT(@RightIdsList, ',');  
 END
		DELETE
		FROM [Security].RightRoute 
		WHERE [Security].RightRoute.RouteId = @RouteId
				
		INSERT INTO [Security].RightRoute(RouteId, RightId, CreatedBy, CreatedDate)
		SELECT @RouteId, RightId, @CreatedBy, @CreatedDate
		FROM @Rights
END

CREATE PROCEDURE [Security].[UpdateRightsAssignedToADGroup](
    @AdGroupId INT,
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
	FROM [Security].ADGroupRight
	WHERE [Security].ADGroupRight.ADGroupId = @AdGroupId
			
	INSERT INTO [Security].ADGroupRight(ADGroupId, RightId, CreatedBy, CreatedDate)
	SELECT @AdGroupId, RightId, @CreatedBy, @CreatedDate
	FROM @Rights
END

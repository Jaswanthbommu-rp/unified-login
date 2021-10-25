CREATE PROCEDURE [Security].[UpdateRightsForRole](
    @RoleId int,
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
	BEGIN TRANSACTION;
		DELETE
		FROM [Security].RoleRight
		WHERE [Security].RoleRight.RoleId = @RoleId
				
		INSERT INTO [Security].RoleRight(RoleId, RightId, CreatedBy, CreatedDate)
		SELECT @RoleId, RightId, @CreatedBy, @CreatedDate
		FROM @Rights
END

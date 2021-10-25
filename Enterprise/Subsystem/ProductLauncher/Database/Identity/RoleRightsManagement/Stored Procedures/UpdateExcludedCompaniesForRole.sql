CREATE PROCEDURE [Security].[UpdateExcludedCompaniesForRole](
    @RoleId int,
	@CompanyIdsList nvarchar(max),
	@CreatedBy INT
)
AS
BEGIN
DECLARE @CreatedDate datetime = GETUTCDATE()
DECLARE @Companies TABLE (  
	CompanyId int PRIMARY KEY  
)  
IF (LEN(@CompanyIdsList) > 0)  
 BEGIN  
  INSERT INTO @Companies (  
   CompanyId  
  )  
  SELECT CONVERT(int, value)  
  FROM STRING_SPLIT(@CompanyIdsList, ',');  
 END
	DELETE
	FROM [Security].OrganizationOverRideRole
	WHERE [Security].OrganizationOverRideRole.RoleId = @RoleId
			
	INSERT INTO [Security].OrganizationOverRideRole(RoleId, OrgPartyId, CreatedBy, CreatedDate)
	SELECT @RoleId, CompanyId, @CreatedBy, @CreatedDate
	FROM @Companies
END
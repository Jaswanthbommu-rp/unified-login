
CREATE PROCEDURE Staging.ListProductUsersByOrganizationId 
(
@OrganizationId INT
)
AS
SELECT 
      [ProductOrganizationId]
      ,[ProductId]
      ,[Title]
      ,[FirstName]
      ,[MiddleName]
      ,[LastName]
      ,[EMail]
      ,[LoginName]
      ,[Phone]
      ,[UserStatus]
      ,Name 'Status'
  FROM [Staging].[ProductUser]
    INNER JOIN Ident.Status s
	   ON Staging.ProductUser.StatusId = s.StatusId
    WHERE s.Name = 'Staged'
    AND Staging.ProductUser.ProductOrganizationId = @OrganizationId

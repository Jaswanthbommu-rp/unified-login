CREATE PROCEDURE [Enterprise].SearchCompanyType (  
  @CompanyTypeId INT = NULL   
 ,@Name NVARCHAR(100) = NULL   
 ,@CreatedDate DATETIME = NULL   
)  
  
AS  
  
BEGIN  
  
 SELECT  
   [OrganizationTypeId] AS CompanyTypeId  
  ,[Name]  
  ,[CreateDate]    
 FROM  
  [Enterprise].[OrganizationType]   
 WHERE   
  (@CompanyTypeId IS NULL  OR  [OrganizationTypeId] = @CompanyTypeId)  
 AND  
  (@Name IS NULL OR [Name] = @Name OR CHARINDEX(@Name,[Name]) > 0)  
 AND  
  (@CreatedDate IS NULL  OR  [CreateDate] = @CreatedDate)  
  
END
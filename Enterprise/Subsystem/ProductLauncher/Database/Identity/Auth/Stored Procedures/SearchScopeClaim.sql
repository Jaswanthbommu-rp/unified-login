CREATE PROCEDURE [Auth].SearchScopeClaim (  
  @ScopeClaimId INT = NULL   
 ,@ScopeId INT = NULL   
 ,@Name NVARCHAR(400) = NULL   
 ,@Description NVARCHAR(2000) = NULL   
 ,@AlwaysIncludeInIdToken BIT = NULL   
)  
  
AS  
  
BEGIN  
  
 SELECT  
   [ScopeClaimId]  
  ,[ScopeId]  
  ,[Name]  
  ,[Description]  
  ,[AlwaysIncludeInIdToken]  
 FROM  
  [Auth].[ScopeClaims]  
 WHERE   
  (@ScopeClaimId IS NULL  OR  [ScopeClaimId] = @ScopeClaimId)  
 AND  
  (@ScopeId IS NULL  OR  [ScopeId] = @ScopeId)  
 AND  
  (@Name IS NULL OR [Name] = @Name OR CHARINDEX(@Name,[Name]) > 0)  
 AND  
  (@Description IS NULL OR [Description] = @Description OR CHARINDEX(@Description,[Description]) > 0)  
 AND  
  (@AlwaysIncludeInIdToken IS NULL  OR  [AlwaysIncludeInIdToken] = @AlwaysIncludeInIdToken)  
  
END
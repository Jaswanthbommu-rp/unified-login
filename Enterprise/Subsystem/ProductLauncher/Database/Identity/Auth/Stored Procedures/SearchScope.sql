-- DEAD
CREATE PROCEDURE [Auth].SearchScope (  
  @ScopeId INT = NULL   
)  
  
AS  
  
BEGIN  
  
	SELECT 
		 [Id]
		,[Enabled]
		,[Name]
		,[DisplayName]
		,[Description]
		,[Required]
		,[Emphasize]
		,[ShowInDiscoveryDocument]
		,[Created]
		,[Updated]
		,[LastAccessed]
		,[NonEditable]
	FROM 
		[Auth].[ApiScopes]
	 WHERE   
	@ScopeId IS NULL OR  [Id] = @ScopeId
END
GO



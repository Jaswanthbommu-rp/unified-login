CREATE PROCEDURE [Auth].SearchScope (  
  @ScopeId INT = NULL   
 ,@Name NVARCHAR(400) = NULL   
 ,@DisplayName NVARCHAR(400) = NULL   
 ,@Description NVARCHAR(2000) = NULL   
 ,@ClaimsRule NVARCHAR(400) = NULL   
 ,@Enabled BIT = NULL   
 ,@Required BIT = NULL   
 ,@Emphasize BIT = NULL   
 ,@Type INT = NULL   
 ,@IncludeAllClaimsForUser BIT = NULL   
 ,@ShowInDiscoveryDocument BIT = NULL   
 ,@AllowUnrestrictedIntrospection BIT = NULL   
)  
  
AS  
  
BEGIN  
  
 SELECT  
   [ScopeId]  
  ,[Name]  
  ,[DisplayName]  
  ,[Description]  
  ,[ClaimsRule]  
  ,[Enabled]  
  ,[Required]  
  ,[Emphasize]  
  ,[Type]  
  ,[IncludeAllClaimsForUser]  
  ,[ShowInDiscoveryDocument]  
  ,[AllowUnrestrictedIntrospection]  
 FROM  
  [Auth].[Scopes]  
 WHERE   
  (@ScopeId IS NULL  OR  [ScopeId] = @ScopeId)  
 AND  
  (@Name IS NULL OR [Name] = @Name OR CHARINDEX(@Name,[Name]) > 0)  
 AND  
  (@DisplayName IS NULL OR [DisplayName] = @DisplayName OR CHARINDEX(@DisplayName,[DisplayName]) > 0)  
 AND  
  (@Description IS NULL OR [Description] = @Description OR CHARINDEX(@Description,[Description]) > 0)  
 AND  
  (@ClaimsRule IS NULL OR [ClaimsRule] = @ClaimsRule OR CHARINDEX(@ClaimsRule,[ClaimsRule]) > 0)  
 AND  
  (@Enabled IS NULL  OR  [Enabled] = @Enabled)  
 AND  
  (@Required IS NULL  OR  [Required] = @Required)  
 AND  
  (@Emphasize IS NULL  OR  [Emphasize] = @Emphasize)  
 AND  
  (@Type IS NULL  OR  [Type] = @Type)  
 AND  
  (@IncludeAllClaimsForUser IS NULL  OR  [IncludeAllClaimsForUser] = @IncludeAllClaimsForUser)  
 AND  
  (@ShowInDiscoveryDocument IS NULL  OR  [ShowInDiscoveryDocument] = @ShowInDiscoveryDocument)  
 AND  
  (@AllowUnrestrictedIntrospection IS NULL  OR  [AllowUnrestrictedIntrospection] = @AllowUnrestrictedIntrospection)  
  
END
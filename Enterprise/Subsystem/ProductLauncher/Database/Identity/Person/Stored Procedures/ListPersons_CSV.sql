  
/*  
 EXEC Person.[ListPersons_CSV] @RealPageId = 'F5C090FA-78AB-452F-B504-98AAFEE09121',--'5c06c1d1-5ca0-4bfc-8919-1b3c940c35d3',   @ParentPartyRoleTypeId = 400,  @UserListFilterType = 0,  @AssignedProducts =N'{"assignedProducts":[{"ColumnName":"ProductId","S
earchValue":"1,3,8,9,14,15,16,27,28,36,38,44,52,56,29,30,31,32,33,34,51,53,54,66"}]}',  @FilterBy ='{"filterBy":[{"ColumnName":"status","SearchValue":"1,23,3,2,12"},{"ColumnName":"offsetMinutes","SearchValue":"0"}]}',  @SortBy =N'{"sortBy":[{"ColumnName":
"firstName","SortDirection":"ASC"}]}',  @RowsPerPage = 100,  @PageNumber =1  
  
*/  
  
CREATE PROCEDURE [Person].[ListPersons_CSV]  
(    
  @RealPageId uniqueidentifier = NULL,    
  @ParentPartyRoleTypeId int = NULL,    
  @UserListFilterType tinyint = 0,    
  @AssignedProducts nvarchar(max),    
  @FilterBy nvarchar(max),    
  @SortBy nvarchar(max),    
  @RowsPerPage int = 0,    
  @PageNumber int = 1    
)    
AS    
BEGIN    
/*  
 DECLARE @RealPageId uniqueidentifier = NULL,    
  @ParentPartyRoleTypeId int = NULL,    
  @UserListFilterType tinyint = 0,    
  @AssignedProducts nvarchar(max),    
  @FilterBy nvarchar(max),    
  @SortBy nvarchar(max),    
  @RowsPerPage int = 0,    
  @PageNumber int = 1    
    
  SELECT  
 @RealPageId = '5c06c1d1-5ca0-4bfc-8919-1b3c940c35d3',  @ParentPartyRoleTypeId = 400,  @UserListFilterType = 0,  @AssignedProducts =N'{"assignedProducts":[{"ColumnName":"ProductId","SearchValue":"1,3,8,9,14,15,16,27,28,36,38,44,52,56,29,30,31,32,33,34,51,
53,54,66"}]}',  @FilterBy ='{"filterBy":[{"ColumnName":"status","SearchValue":"1,23,3,2,12"},{"ColumnName":"offsetMinutes","SearchValue":"0"}]}',  @SortBy =N'{"sortBy":[{"ColumnName":"firstName","SortDirection":"ASC"}]}',  @RowsPerPage = 4000,  @PageNumbe
r =1   
 */  
  
  DECLARE @PartyId BIGINT  
  
  SELECT @PartyId = PartyId    
  FROM  Enterprise.Party    
  WHERE RealPageId = @RealPageId    
  
  
 DROP TABLE IF EXISTS #Temp_Person_ListPersons_Ver04  
  
 CREATE TABLE #Temp_Person_ListPersons_Ver04  
 (  
  ID INT IDENTITY(1,1) PRIMARY KEY,  
  TotalRecords INT,   
  RealPageID UNIQUEIDENTIFIER,  
  PartyId BIGINT,  
  FirstName NVARCHAR(100),  
  MiddleName NVARCHAR(100),  
  LastName NVARCHAR(100),  
  EmployeeId NVARCHAR(400),  
  Title  NVARCHAR(100),  
  Suffix  NVARCHAR(20),  
  CustomField NVARCHAR(256),  
  UserId BIGINT,  
  LoginName NVARCHAR(256),  
  LastLogin DATETIME,  
  FromDate DATETIME,  
  ThruDate DATETIME,  
  StatusId INT,  
  StatusName NVARCHAR(50),  
  StatusThruDate DATETIME,  
  Is3rdPartyIDP INT,  
  PasswordModifiedDate DATETIME,   
  OffsetMinutes INT,  
  EntepriseRoleName NVARCHAR(256),  
  EntepriseRoleId INT,  
  Products INT,   
  Properties INT,   
  UserType  NVARCHAR(100),  
  PartyRoleTypeId INT  
    
 )  
   
 INSERT INTO #Temp_Person_ListPersons_Ver04  
 (  
  TotalRecords    
  ,RealPageID   
  ,PartyId    
  ,FirstName   
  ,MiddleName   
  ,LastName   
  ,EmployeeId   
  ,Title    
  ,Suffix    
  ,CustomField   
  ,UserId    
  ,LoginName   
  ,LastLogin   
  ,FromDate   
  ,ThruDate   
  ,StatusId   
  ,StatusName   
  ,StatusThruDate   
  ,Is3rdPartyIDP   
  ,PasswordModifiedDate    
  ,OffsetMinutes   
  ,EntepriseRoleName   
  ,EntepriseRoleId   
  ,Products    
  ,Properties    
  ,UserType  
  ,PartyRoleTypeId  
 )  
 EXEC Person.ListPersons_Ver04 @RealPageId ,  @ParentPartyRoleTypeId ,   @UserListFilterType ,  @AssignedProducts ,  @FilterBy ,  @SortBy ,  @RowsPerPage ,  @PageNumber   
  
 DROP TABLE IF EXISTS #PartyContactMechanism  
  
 CREATE TABLE #PartyContactMechanism  
 (  
  PartyId BIGINT PRIMARY KEY,  
  ContactMechanismId INT,  
  RowNo INT  
 )  
  
 INSERT INTO #PartyContactMechanism(PartyId,ContactMechanismId,RowNo)  
 SELECT PartyId,ContactMechanismId,RowNo   
 FROM  
 (  
  SELECT PartyId,ContactMechanismId,ROW_NUMBER() OVER(PARTITION BY PartyId ORDER BY FromDate DESC) AS RowNo  
  FROm Enterprise.PartyContactMechanism  
  WHERE ThruDate > GETUTCDATE()  
 ) X  
 WHERE X.RowNo = 1  
  
   
 DROP TABLE IF EXISTS #ProductEnabled  
 DROP TABLE IF EXISTS ##Temp_ProductEnabled  
  
 CREATE TABLE #ProductEnabled  
 (  
  ID INT IDENTITY(1,1) PRIMARY KEY,  
  UserId INT NOT NULL,  
  PersonaId INT NOT NULL ,  
  ProductId INT NOT NULL,  
  ProductName  NVARCHAR(100),  
  ProductEnabled VARCHAR(8)  
 )  
  
    
 ;WITH CTE_Org_Products AS  
 (  
  --Each Organisation wise productIds and getting personaId for dynamic columns  
  SELECT DISTINCT  
   ulp.UserLoginId AS UserId,OP.PartyId,P.PersonaId,Op.ProductId ,Pr.[Name] as ProductName   
  FROM   
   Enterprise.OrganizationProduct OP  
  INNER JOIN Enterprise.Product Pr On OP.ProductId = Pr.ProductId  
  INNER JOIN Ident.UserLoginPersona ULP ON OP.PartyId = ULP.OrganizationPartyId  
  INNER JOIN Person.Persona p ON ULP.UserLoginPersonaId = p.UserLoginPersonaId  
  WHERE  
   PartyId = @PartyId   
 )  
 ,CTE_Persona_Products AS   
 (  
  --Each personaId and their linked products  
  SELECT DISTINCT   
  ulp.OrganizationPartyId,P.PersonaId  
  ,pec.ProductId,Pr.[Name] as ProductName,  
  'Y' AS ProductEnabled  
  FROM   
  Person.Persona p  
  INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = p.UserLoginPersonaId  
  INNER JOIN Enterprise.PersonaConfiguration pec ON p.PersonaId = pec.PersonaId  
  INNER JOIN Enterprise.Product Pr On Pec.ProductId = Pr.ProductId  
  WHERE  
  OrganizationPartyId = @PartyId   
 )  
 INSERT INTO #ProductEnabled  
 (  
  UserId,PersonaId,ProductId,ProductName,ProductEnabled  
 )  
 SELECT   
  OP.UserId,OP.PersonaId,OP.ProductId,OP.ProductName,ISNULL(PP.ProductEnabled ,'N') AS ProductEnabled  
 FROM   
  CTE_Org_Products OP  
 LEFT JOIN  
  CTE_Persona_Products PP ON OP.PartyId = PP.OrganizationPartyId AND OP.PersonaId = PP.PersonaId AND OP.ProductId = PP.ProductId  
  
  
   
 DECLARE @Columns NVARCHAR(MAX), @query NVARCHAR(MAX),@SelectColumnNames NVARCHAR(MAX);  
 SET @Columns = STUFF((SELECT DISTINCT ','+QUOTENAME(ProductName) FROM #ProductEnabled FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, '');  
  
 SET @SelectColumnNames = STUFF((SELECT DISTINCT ','+ 'ISNULL('+QUOTENAME(ProductName)+ ', ''N'') AS '+ QUOTENAME(ProductName)   
         FROM #ProductEnabled c FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, '');  
  
   
 SET @query = 'SELECT   
    UserId,'+@SelectColumnNames+'  
    INTO ##Temp_ProductEnabled  
    FROM   
    (SELECT UserId,ProductName,ProductEnabled   
    FROM  #ProductEnabled PE  
    )x   
    pivot (  
     max(ProductEnabled) for ProductName in ('+@Columns+')  
    ) p';  
  
 EXEC sp_executesql @query;  
  
 SELECT   
   T.TotalRecords    
  ,T.RealPageID   
  ,T.PartyId    
  ,T.FirstName   
  ,T.MiddleName   
  ,T.LastName   
  ,T.EmployeeId   
  ,T.Title    
  ,T.Suffix    
  ,T.CustomField   
  ,T.UserId    
  ,T.LoginName   
  ,T.LastLogin   
  ,T.FromDate   
  ,T.ThruDate   
  ,T.StatusId   
  ,T.StatusName   
  ,T.StatusThruDate   
  ,T.Is3rdPartyIDP   
  ,T.PasswordModifiedDate    
  ,T.OffsetMinutes   
  ,T.EntepriseRoleName   
  ,T.EntepriseRoleId   
  ,T.Products    
  ,T.Properties    
  ,T.UserType  
  ,T.PartyRoleTypeId  
  ,EA.ElectronicAddressString AS NotificationEmail  
  , CASE   
   WHEN ul.TwoFactorEnabled = 1 THEN 'Yes'  
   WHEN ul.TwoFactorEnabled = 0 THEN 'No'  
    END AS MFAFlag  
  ,R.RoleName AS PlatformRole  
  ,PRE.*  
 FROM #Temp_Person_ListPersons_Ver04 T  
 LEFT JOIN #PartyContactMechanism PCM ON T.PartyId = PCM.PartyId  
 LEFT JOIN Enterprise.ElectronicAddress EA ON PCM.ContactMechanismId=EA.ContactMechanismID    
 LEFT JOIN Ident.UserLogin ul ON T.UserId = ul.UserId    
 LEFT JOIN Ident.UserLoginPersona iulp ON iulp.UserLoginId = ul.UserId    
 LEFT JOIN  Person.Persona PE  ON (pe.UserLoginPersonaId = iulp.UserLoginPersonaId)   
 LEFT JOIN [Security].[PersonaRole] PR ON pe.PersonaId = PR.PersonaId  
 LEFT JOIN [Security].[Role] R ON PR.RoleId = R.RoleId  
 LEFT JOIN  ##Temp_ProductEnabled PRE ON T.UserId = PRE.UserId   
  
  
 DROP TABLE IF EXISTS #PartyContactMechanism  
 DROP TABLE IF EXISTS #Temp_Person_ListPersons_Ver04  
 DROP TABLE IF EXISTS #ProductEnabled  
 DROP TABLE IF EXISTS ##Temp_ProductEnabled  
  
  
END;
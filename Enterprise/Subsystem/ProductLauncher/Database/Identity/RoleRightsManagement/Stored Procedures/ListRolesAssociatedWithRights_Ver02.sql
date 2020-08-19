CREATE PROCEDURE [Security].[ListRolesAssociatedWithRights_Ver02] (  
	@Partyid INT  
	,@ProductId INT  
	,@TargetProductId varchar(max)  
	,@RoleId INT = NULL  
 )  
AS  
BEGIN  
 IF ((LEN(@TargetProductId) = 0) OR (ISJSON(@TargetProductId) = 0))  
 BEGIN  
  SELECT 0 AS Id  
   ,'Target ProductId list is empty.';  
  RETURN;  
 END;  
 Declare @VisibleStatusId INT  
  
 Declare @OrgDefaultRole AS TABLE (  
   RoleId int,  
   DefaultRole bit default 0);  
  
 DECLARE @TargetProductIds TABLE (  
  ProductId int PRIMARY KEY  
 )  
  
 Select @VisibleStatusId = StatusTypeId  
 From Enterprise.StatusType  
 Where Name = 'All'  
  
 Insert Into @OrgDefaultRole(  
  RoleId,  
  DefaultRole  
 )  
 Select RoleId,  
    1  
 From Security.OrganizationDefaultRole  
 Where OrgPartyId = @PartyId  
  
 SELECT @TargetProductId = ColumnValue  
 FROM OPENJSON (JSON_QUERY(@TargetProductId, '$.targetProducts'))  
 WITH (  
     ColumnValue nvarchar(max) '$.productIds'  
    )  
  
 INSERT INTO @TargetProductIds (  
  ProductId  
 )  
 SELECT CONVERT(int, value)  
 FROM  STRING_SPLIT(@TargetProductId, ',');  
    
 SELECT DISTINCT R.rolename [Role]  
  ,R.ShortName [RoleNickName]  
  ,r.RoleID [RoleId]  
  ,RG.Value [Right]  
  ,RG.RightName [RightNickName]  
  ,ISNULL(RG.RightID, 0) [RightId]  
  ,ISNULL(RG.RightId, 0) [RightValueTypeId]  
  ,RT.Value [RoleType]  
  ,ISNULL(OD.DefaultRole, 0) AS DefaultRole    
 FROM [Security].ROLE r  
 INNER JOIN [Security].RoleType AS RT  ON  
  RT.RoleTypeId = R.RoleTypeID    
 LEFT OUTER JOIN [Security].RoleRight RR ON   
  RR.RoleId = R.RoleId  
 LEFT OUTER JOIN [Security].[Right] RG ON   
  RG.RightId = RR.RightId  
 LEFT OUTER JOIN [Security].OrganizationOverRideRight ORR ON  
  ORR.RightId = RG.RightId  
  And ORR.OrgPartyId = @PartyID  
 LEFT JOIN @OrgDefaultRole AS OD ON  
  OD.RoleId = R.RoleId  
 LEFT OUTER JOIN @TargetProductIds tp ON (RG.TargetProductId = tp.ProductId)   
 WHERE R.OrgPartyID = @Partyid  
  AND R.ProductId = @ProductId  
  AND (R.RoleID = @RoleId OR @RoleId IS NULL)   
  AND R.RoleID <> - 1  
  AND ((RG.VisibilityStatusId IS NULL OR RG.VisibilityStatusId = @VisibleStatusId) OR  ORR.VisibilityStatusId = @VisibleStatusId)  
 UNION  
 SELECT DISTINCT R.rolename [Role]  
  ,R.ShortName [RoleNickName]  
  ,r.RoleID [RoleId]  
  ,RG.Value [Right]  
  ,RG.RightName [RightNickName]  
  ,RG.RightID AS [RightId]  
  ,RG.RightId AS [RightValueTypeId]  
  ,RT.Value [RoleType]  
  ,ISNULL(OD.DefaultRole, 0) AS DefaultRole    
 FROM [Security].ROLE r  
 INNER JOIN [Security].RoleType AS RT  ON  
  RT.RoleTypeId = R.RoleTypeID    
 INNER JOIN [Security].RoleRight RR ON   
  RR.RoleId = R.RoleId  
 INNER JOIN [Security].[Right] RG ON   
  RG.RightId = RR.RightId  
 INNER JOIN @TargetProductIds tp ON (RG.TargetProductId = tp.ProductId)   
 LEFT OUTER JOIN [Security].OrganizationOverRideRight ORR ON  
  ORR.RightId = RG.RightId  
  And ORR.OrgPartyId = @PartyID  
 LEFT JOIN @OrgDefaultRole AS OD ON  
  OD.RoleId = R.RoleId  
 WHERE R.OrgPartyID IS NULL  
  AND R.ProductId = @ProductId  
  AND (R.RoleID = @RoleId OR @RoleId IS NULL)   
  AND (RG.VisibilityStatusId = @VisibleStatusId OR  ORR.VisibilityStatusId = @VisibleStatusId)  
END  

GO

/*
sp_helptext 'Auth.GetUserClaimTypesRequiredForClient'


exec Auth.GetUserClaimTypesRequiredForClient @clientname = 'integrationmarketplace'
exec Auth.GetUserClaimTypesRequiredForClient @clientname = 'greenbookoidc'
exec Auth.GetUserClaimTypesRequiredForClient @clientname = 'blackbook'
exec Auth.GetUserClaimTypesRequiredForClient @clientname = 'UNIFIEDAMENITIES'

exec Auth.GetUserClaimTypesRequiredForClient @clientname = 'cimplapi-swagger'
exec Auth.GetUserClaimTypesRequiredForClient @clientname = 'cimpl-ui'
exec Auth.GetUserClaimTypesRequiredForClient @clientname = 'cimplapitest'
exec Auth.GetUserClaimTypesRequiredForClient @clientname = 'cimpl-server'

exec Auth.GetUserClaimTypesRequiredForClient @clientname = 'createcompany'

select * from auth.clients where clientcode like 'cimpl%'


exec Auth.GetUserClaimTypesRequiredForClient @clientname = 'CREATECOMPANY'

select * from auth.clientuserclaim 

select * from auth.ClientUserClaim where clientid = 188
select * from auth.Clients where ClientCode like 'VENDORCOMPLIANCE%'
exec Auth.GetUserClaimTypesRequiredForClient @clientname = 'vendorcompliance'
exec Auth.GetUserClaimTypesRequiredForClient @clientname = 'vendorcomplianceui'
exec Auth.GetUserClaimTypesRequiredForClient @clientname = 'cimplapitest'


select * from auth.clientuserclaim where clientuserclaimid = 192
select * from auth.claim

insert into auth.clientuserclaim ( clientid, claimid ) values ( 192, 8 )
delete from auth.ClientUserClaim  where clientid = 192 and clientuserclaimid = 57
select * from auth.ClientUserClaim where clientid = 192
update auth.claim set claimname = 'accounting-companyname', samlattributename = 'UserId~|0' where claimid = 8

insert into auth.clientuserclaim ( clientid, claimid ) values ( 188, 4 )
insert into auth.clientuserclaim ( clientid, claimid ) values ( 188, 7 )


insert into auth.claim ( claimname, samlattributename, productid ) values ( 'role|rights', '', 3 )

update auth.clientuserclaim set claimid = 24 where clientuserclaimid = 49
update auth.clientuserclaim set claimid = 20 where clientuserclaimid = 49

update auth.claim set claimname = 'phonenumber' where claimid = 24

delete from auth.clientuserclaim where clientuserclaimid = 49

select c.clientcode,cc.*, cl.*
	FROM Auth.Clients C
	INNER JOIN Auth.ClientUserClaim AS cc ON C.ClientId = CC.ClientId
	INNER JOIN Auth.Claim CL ON CL.ClaimId = CC.ClaimId

select * from auth.clients where clientcode like 'cimpl%'
select * from auth.Claim
*/

if not exists ( select top 1 1 from auth.claim )
begin
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'userId', '', 3 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'productuserid', 'UserId', 18 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'userId', 'UserId', 40 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'userId', 'UserId', 41 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'os-userinfo', 'UserId', 1 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'pam-username', 'productUsername', 44 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'pam-orgid', 'PMCID', 44 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'onesite-pmcid', 'PMCID', 1 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'accounting-companyname', 'UserId~|0', 8 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'im-role', 'RoleCode', 39 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'ao-username', 'productUsername', 4 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'diq-userid', 'productUsername', 47 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'clkpay-username', 'productUsername', 48 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'ops-buyer-username', 'productUsername', 13 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'vendorcompliance-username', 'productUsername', 16 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'user_id', 'UserId', 23 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'company_id', 'PMCID', 23 )
	--insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'os-userinfo', 'UserId', 26 )

	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'role', '', 3 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'role|rolealias', '', 24 )
	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'role|rights', '', 26 )

	insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'phonenumber', '', 3 )
end

if exists ( select top 1 1 from auth.clients where clientcode = 'UM-USERMGMT-SWAGGER' ) AND not exists (SELECT top 1 1 from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'UM-USERMGMT-SWAGGER') 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'UM-USERMGMT-SWAGGER' and c1.ProductId = 18
end

if exists ( select top 1 1 from auth.clients where clientcode = 'LEADMANAGEMENT' ) AND not exists (SELECT top 1 1 from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'LEADMANAGEMENT') 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'LEADMANAGEMENT' and c1.ProductId = 40
end

if exists ( select top 1 1 from auth.clients where clientcode = 'LEADANALYTICS' ) AND not exists (SELECT top 1 1 from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'LEADANALYTICS') 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'LEADANALYTICS' and c1.ProductId = 41
end

if exists ( select top 1 1 from auth.clients where clientcode = 'FACILITIES-PLUS' ) AND not exists (SELECT top 1 1 from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'FACILITIES-PLUS') 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'FACILITIES-PLUS' and c1.ProductId = 1
end

if exists ( select top 1 1 from auth.clients where clientcode = 'PORTFOLIOMANAGEMENT' ) AND not exists (SELECT top 1 1 from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'PORTFOLIOMANAGEMENT') 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'PORTFOLIOMANAGEMENT' and c1.ProductId = 44
end

if exists ( select top 1 1 from auth.clients where clientcode = 'PIM' ) AND not exists (SELECT top 1 1 from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'PIM') 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'PIM' and c1.ProductId = 44
end

if exists ( select top 1 1 from auth.clients where clientcode = 'INTEGRATIONMARKETPLACE' ) AND not exists (SELECT top 1 1 from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'INTEGRATIONMARKETPLACE') 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'INTEGRATIONMARKETPLACE' and c1.ProductId in (1, 8, 39 )
end

if exists ( select top 1 1 from auth.clients where clientcode = 'PURCHASINGPORTAL' ) AND not exists (SELECT top 1 1 from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'PURCHASINGPORTAL') 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'PURCHASINGPORTAL' and c1.ProductId = 8
end

if exists ( select top 1 1 from auth.clients where clientcode = 'ONSITE' ) AND not exists (SELECT top 1 1 from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'ONSITE') 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'ONSITE' and c1.ProductId = 23
end

if exists ( select top 1 1 from auth.clients where clientcode = 'ASSETOPTIMIZATION' ) AND not exists (SELECT top 1 1 from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'ASSETOPTIMIZATION') 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'ASSETOPTIMIZATION' and c1.ProductId = 4
end

if exists ( select top 1 1 from auth.clients where clientcode = 'DEPOSITALTERNATIVE' ) AND not exists (SELECT top 1 1 from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'DEPOSITALTERNATIVE') 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'DEPOSITALTERNATIVE' and c1.ProductId = 47
end

if exists ( select top 1 1 from auth.clients where clientcode = 'CLICKPAY' ) AND not exists (SELECT top 1 1 from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'CLICKPAY') 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'CLICKPAY' and c1.ProductId = 48
end

if exists ( select top 1 1 from auth.clients where clientcode like 'ops-buyer%'  ) AND not exists (SELECT top 1 1 from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode like 'ops-buyer%' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode like 'ops-buyer%'  and c1.ProductId = 13
end

if exists ( select top 1 1 from auth.clients where clientcode like 'onesite%'  ) AND not exists (SELECT top 1 1 from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode like 'onesite%' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode like 'onesite%'  and c1.ProductId = 1
end

if exists ( select top 1 1 from auth.clients where clientcode like 'UNIFIEDAMENITIES%'  ) AND not exists (SELECT top 1 1 from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode like 'UNIFIEDAMENITIES%' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode like 'UNIFIEDAMENITIES%'  and c1.ProductId = 26
end

if exists ( select top 1 1 from auth.clients where clientcode like 'VENDORCOMPLIANCE%'  ) AND not exists (SELECT top 1 1 from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode like 'VENDORCOMPLIANCE%' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode like 'VENDORCOMPLIANCE%'  and c1.ProductId = 16
end

if exists ( select top 1 1 from auth.clients where clientcode like 'UNIFIEDAMENITIES%'  ) AND not exists (SELECT * from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode like 'UNIFIEDAMENITIES%' inner join auth.Claim c2 on c1.ClaimId = c2.ClaimId and c2.ProductId = 3 and c2.SAMLAttributeName = '' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode like 'UNIFIEDAMENITIES%'  and c1.ProductId = 3 and c1.claimname = 'userid'
end

if exists ( select top 1 1 from auth.clients where clientcode like 'UNIFIEDAMENITIES%'  ) AND not exists (SELECT * from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode like 'UNIFIEDAMENITIES%' inner join auth.Claim c2 on c1.ClaimId = c2.ClaimId and c2.ProductId = 1 and c2.claimname = 'os-userinfo' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode like 'UNIFIEDAMENITIES%'  and c1.ProductId = 1 and c1.claimname = 'os-userinfo'
end


if exists ( select top 1 1 from auth.clients where clientcode like 'blackbook%'  ) AND not exists (SELECT * from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode like 'blackbook%' inner join auth.Claim c2 on c1.ClaimId = c2.ClaimId and c2.ProductId = 3 and c2.SAMLAttributeName = '' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode like 'blackbook%'  and c1.ProductId = 3 and c1.claimname = 'userid'
end

if exists ( select top 1 1 from auth.clients where clientcode = 'landing'  ) AND not exists (SELECT * from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'landing' inner join auth.Claim c2 on c1.ClaimId = c2.ClaimId and c2.ProductId = 3 and c2.SAMLAttributeName = '' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'landing'  and c1.ProductId = 3
end

if exists ( select top 1 1 from auth.clients where clientcode = 'rplandingapi'  ) AND not exists (SELECT * from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'rplandingapi' inner join auth.Claim c2 on c1.ClaimId = c2.ClaimId and c2.ProductId = 3 and c2.SAMLAttributeName = '' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'rplandingapi'  and c1.ProductId = 3
end

if exists ( select top 1 1 from auth.clients where clientcode = 'qaautomation'  ) AND not exists (SELECT * from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'qaautomation' inner join auth.Claim c2 on c1.ClaimId = c2.ClaimId and c2.ProductId = 3 and c2.SAMLAttributeName = '' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'qaautomation'  and c1.ProductId = 3
end

if exists ( select top 1 1 from auth.clients where clientcode like 'migration%'  ) AND not exists (SELECT * from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode like 'migration%' inner join auth.Claim c2 on c1.ClaimId = c2.ClaimId and c2.ProductId = 3 and c2.SAMLAttributeName = '' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode like 'migration%'  and c1.ProductId = 3
end

if exists ( select top 1 1 from auth.clients where clientcode = 'settings-management'  ) AND not exists (SELECT * from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'settings-management' inner join auth.Claim c2 on c1.ClaimId = c2.ClaimId and c2.ProductId = 3 and c2.SAMLAttributeName = '' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'settings-management'  and c1.ProductId = 3
end

if exists ( select top 1 1 from auth.clients where clientcode = 'greenbookoidc'  ) AND not exists (SELECT * from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'greenbookoidc' inner join auth.Claim c2 on c1.ClaimId = c2.ClaimId and c2.ProductId = 3 and c2.SAMLAttributeName = '' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'greenbookoidc'  and c1.ProductId = 3
end

if exists ( select top 1 1 from auth.clients where clientcode = 'createcompany'  ) AND not exists (SELECT * from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'createcompany' inner join auth.Claim c2 on c1.ClaimId = c2.ClaimId and c2.ProductId = 3 and c2.SAMLAttributeName = '' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'createcompany'  and c1.ProductId = 3
end

if exists ( select top 1 1 from auth.clients where clientcode = 'blackbook'  ) AND not exists (SELECT * from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'blackbook' inner join auth.Claim c2 on c1.ClaimId = c2.ClaimId and c2.ProductId = 24 and c2.SAMLAttributeName = '' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'blackbook'  and c1.ProductId = 24
end

if exists ( select top 1 1 from auth.clients where clientcode = 'integrationmarketplace'  ) AND not exists (SELECT * from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode = 'integrationmarketplace' inner join auth.Claim c2 on c1.ClaimId = c2.ClaimId and c2.ProductId = 3 and c2.SAMLAttributeName = '' and ClaimName = 'phonenumber' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode = 'integrationmarketplace'  and c1.ProductId = 3 and ClaimName = 'phonenumber'
end

if exists ( select top 1 1 from auth.clients where clientcode like 'cimpl%'  ) AND not exists (SELECT * from auth.clients c inner join auth.ClientUserClaim c1 on c.clientid = c1.clientid and c.clientcode like 'cimpl%' inner join auth.Claim c2 on c1.ClaimId = c2.ClaimId and c2.ProductId = 3 and c2.SAMLAttributeName = '' and ClaimName = 'role' ) 
begin
	insert into auth.ClientUserClaim ( ClientId, ClaimId ) 
		SELECT clientid, c1.ClaimId from auth.clients c cross join auth.claim c1  where c.clientcode like 'cimpl%'  and c1.ProductId = 3 and ClaimName = 'role'
end

--select * from auth.claim
GO

DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @PartyId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;
DECLARE @TRightShortName NVARCHAR(100)
DECLARE @TargetProductId INT;
DECLARE @VisibilityStatusId INT
IF OBJECT_ID('tempdb..#RightsUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #RightsUnifiedSettings;
END;

IF OBJECT_ID('tempdb..#HoldPartyForUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyForUnifiedSettings;
END;

CREATE TABLE #RightsUnifiedSettings
( 
			 RightId int, Name nvarchar(500), description nvarchar(500), shortname varchar(100)
);

INSERT INTO #RightsUnifiedSettings( rightid, name, description, shortname )
VALUES( 1, 'Access to Submit questionnaires within CIMPL', 'Access to Submit questionnaires within CIMPL', 'CIMPLESubmitQuestionnaires' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

--select * from enterprise.product where name like '%lead%'

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'CIMPL';

SET @ActionValueID = 1

SELECT @RoleCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Role Type' AND 
	  ST.Name = 'System';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'System';


SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
	 JOIN
	 Enterprise.StatusTypeCategory
	 ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification
	 ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType
	 ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'ALL' AND 
	  StatusTypeCategoryType.Name = 'Security';

SET @VisibilityStatusId = @Status;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'CIMPLE Submit Questionnaires' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'CIMPLE Submit Questionnaires', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'Dashboard' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'CIMPLE Submit Questionnaires' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'CIMPLE Submit Questionnaires', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, o.PartyId OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Enterprise.Organization o
	 INNER JOIN Enterprise.Party p

	 ON P.partyid  = O.PartyId
WHERE O.Name <> 'RealPage Employee'

WHILE EXISTS
(
	SELECT 1
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0;
	SELECT @RoleId = RoleId
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.Value = 'User Administrator' AND 
		  R.PartyId = @PartyId;
	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #RightsUnifiedSettings;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_CIMPLESubmitQuestionnaires', @ShortName = 'CIMPLESubmitQuestionnaires', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'CIMPLE Submit Questionnaires'  AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_CIMPLESubmitQuestionnaires';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'CIMPLE Submit Questionnaires'   AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;



		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	UPDATE #HoldPartyForUnifiedSettings
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

DECLARE @Dashboard int;

SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_CIMPLESubmitQuestionnaires';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Access to Submit questionnaires within CIMPL' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @DashBoard
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @DashBoard );

END;
GO
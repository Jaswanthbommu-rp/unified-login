
GO
sp_helptext 'Auth.GetUserClaimTypesRequiredForClient'

sp_helptext 'Enterprise.GetRolesForProductsByPersonaId'


exec Auth.GetUserClaimTypesRequiredForClient @clientname = 'onesite'


select * from auth.Claim
select * from enterprise.product


insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'productuserid', 'UserId', 18 )
insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'userId', 'UserId', 40 )
insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'userId', 'UserId', 41 )
insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'os-userinfo', 'UserId', 1 )
insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'pam-username', 'productUsername', 44 )
insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'pam-orgid', 'PMCID', 44 )
insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'onesite-pmcid', 'PMCID', 1 )
insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'accounting-userinfo', 'UserId', 8 )
insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'im-role', 'RoleCode', 39 )
insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'ao-username', 'productUsername', 4 )
insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'diq-userid', 'productUsername', 47 )
insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'clkpay-username', 'productUsername', 48 )
insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'ops-buyer-username', 'productUsername', 13 )
insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'vendorcompliance-username', 'productUsername', 16 )
insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'user_id', 'UserId', 23 )
insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'company_id', 'PMCID', 23 )
insert into auth.claim ( ClaimName, SAMLAttributeName, ProductId ) values ( 'os-userinfo', 'UserId', 26 )

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




select * from auth.clients where clientcode like 'VENDORCOMPLIANCE%' 

select * from enterprise.product

SELECT CL.ClaimName
		  ,CL.SAMLAttributeName
		  ,CL.ProductId 
	FROM Auth.Clients C
	INNER JOIN Auth.ClientUserClaim AS cc ON C.ClientId = CC.ClientId
	INNER JOIN Auth.Claim CL ON CL.ClaimId = CC.ClaimId
	WHERE C.ClientCode= @ClientName
	

select c.clientcode,cc.*, cl.*
	FROM Auth.Clients C
	INNER JOIN Auth.ClientUserClaim AS cc ON C.ClientId = CC.ClientId
	INNER JOIN Auth.Claim CL ON CL.ClaimId = CC.ClaimId


select * from Auth.ClientUserClaim cc
	INNER JOIN Auth.Claim CL ON CL.ClaimId = CC.ClaimId

select * from auth.clientgroup
select * from auth.Claim
select * from ident.personaclient
update auth.claim set productid = 1 where claimid = 4

 SELECT   
   [Role].[RoleId]  
  ,[Role].[RoleName]  
  ,[Role].[Description]  
  ,[Role].[CreatedBy]  
  ,[Role].[CreatedDate]  
 FROM [Security].[Role]  
 INNER JOIN [Security].[PersonaRole] ON [PersonaRole].[RoleId] = [Role].[RoleId]  
 LEFT OUTER JOIN [Security].[RoleAlias] ON [Role].RoleId = [RoleAlias].RoleId  
 INNER JOIN [Security].[ProductRole] ON [ProductRole].[RoleId] = [Role].[RoleId]  
 INNER JOIN [Enterprise].[Product] ON [Product].[ProductId] = [ProductRole].[ProductId]  
 WHERE [PersonaRole].[PersonaId] = @PersonaId  
 AND [Product].[ProductId] = @ProductId;  
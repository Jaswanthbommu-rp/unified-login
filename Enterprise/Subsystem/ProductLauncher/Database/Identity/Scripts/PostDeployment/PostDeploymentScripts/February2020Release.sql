
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

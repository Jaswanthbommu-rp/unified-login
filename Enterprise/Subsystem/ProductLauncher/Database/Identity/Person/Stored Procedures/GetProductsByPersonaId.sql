CREATE PROCEDURE [Person].[GetProductsByPersonaId]
	@PersonaId int = 0,
	@StatusTypeId int = 8
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @NOW DATETIME= GETUTCDATE();
	DECLARE @CompanyOrganizationProduct TABLE ( ProductId INT ) 
		 
	INSERT INTO @CompanyOrganizationProduct ( ProductId )
	SELECT DISTINCT ProductId from Enterprise.OrganizationProduct OP 
		INNER JOIN Ident.UserLoginPersona ULP ON ULP.OrganizationPartyId = OP.PartyId
		INNER JOIN Person.Persona per ON (ULP.UserLoginPersonaId = per.UserLoginPersonaId and per.PersonaId = @PersonaId)
			AND ((@NOW BETWEEN op.FromDate AND op.ThruDate) OR (@NOW >= op.FromDate AND op.ThruDate IS NULL))
	UNION
	SELECT ProductId FROM Enterprise.Product Where AssignToAllUsers = 1

	if 2 = ( select count(1) from @CompanyOrganizationProduct WHERE ProductId in ( 19, 36 ) )
	begin
		delete from @CompanyOrganizationProduct where ProductId = 19
	end

	IF EXISTS ( SELECT TOP 1 1 FROM @CompanyOrganizationProduct Where ProductID = 4 )
	BEGIN
		INSERT INTO @CompanyOrganizationProduct ( ProductId )
			Select ProductId from Enterprise.Product where ProductTypeId IN ( SELECT ProductTypeId FROM Enterprise.ProductType where ParentProductTypeId = 400 )
	END	

	;with ProductSettings AS (
		select ps.productid, pst.name, ps.value from enterprise.ProductSetting ps inner join enterprise.ProductSettingType pst on ps.ProductSettingTypeId = pst.ProductSettingTypeId where pst.name in ( 'isresource', 'isnewtab', 'ProductUrl', 'ShowInAppSwitcher' )
	)
	SELECT 
		PC.ProductId
		,P.Name
		,P.BooksProductCode
		,CONVERT(TINYINT, PC.isFavorite) as isFavorite
		,PC.StatusTypeId
		,P.Description
		,PT.ParentProductTypeId as FamilyId
		,PT2.Name as FamilyName
		,CONVERT(TINYINT,ISNULL(ps1.value,0)) as IsNewTab
		,CONVERT(TINYINT,ISNULL(ps2.value,0)) as IsResource
		,PS3.Value as Url
		,CONVERT(TINYINT,ISNULL(ps4.value,0)) as ShowInAppSwitcher
		--,*
	FROM
		Enterprise.PersonaConfiguration PC
		INNER JOIN Enterprise.Product P  ON PC.ProductId = P.ProductId
		LEFT OUTER JOIN enterprise.producttype pt on p.ProductTypeId = pt.ProductTypeId
		LEFT OUTER JOIN Enterprise.ProductType PT2 on PT.ParentProductTypeId = PT2.ProductTypeId
		INNER JOIN @CompanyOrganizationProduct OP on P.ProductId = OP.ProductId
		LEFT OUTER JOIN ProductSettings ps1 on ps1.ProductId = p.ProductId and ps1.name = 'IsNewTab'
		LEFT OUTER JOIN ProductSettings ps2 on ps2.ProductId = p.ProductId and ps2.name = 'IsResource'
		LEFT OUTER JOIN ProductSettings ps3 on ps3.ProductId = p.ProductId and ps3.name = 'ProductUrl'
		LEFT OUTER JOIN ProductSettings ps4 on ps4.ProductId = p.ProductId and ps4.name = 'ShowInAppSwitcher'
	where 
		PC.PersonaId = @PersonaId
		AND PC.ThruDate IS NULL
		AND PC.StatusTypeId = @StatusTypeId
	UNION
	SELECT
		pr.ProductId
		,P.Name
		,P.BooksProductCode
		,CONVERT(TINYINT, 0) as isFavorite -- need to figure out
		,8 as StatusTypeId
		,P.Description
		,PT.ParentProductTypeId as FamilyId
		,PT2.Name as FamilyName
		,CONVERT(TINYINT,ISNULL(ps1.value,0)) as IsNewTab
		,CONVERT(TINYINT,ISNULL(ps2.value,0)) as IsResource
		,PS3.Value as Url
		,CONVERT(TINYINT,ISNULL(ps4.value,0)) as ShowInAppSwitcher
	FROM 
		Enterprise.PersonaPrivilege ppv
        INNER JOIN Enterprise.Role r ON R.RoleID = ppv.RoleID
        INNER JOIN Enterprise.[Right] r2 ON r.RoleID = r2.RoleID
        INNER JOIN Enterprise.RightValueType rvt ON r2.RightValueTypeId = rvt.RightValueTypeId
		INNER JOIN Enterprise.ProductRight PR on PR.RightShortName = rvt.ShortName AND ( PR.DependantProductId is null OR PR.DependantProductId in ( SELECT ProductId FROM Enterprise.PersonaConfiguration WHERE PersonaId = PPV.PersonaId AND StatusTypeId = 8 ))
		INNER JOIN Enterprise.Product P  ON PR.ProductId = P.ProductId
		LEFT OUTER JOIN enterprise.producttype pt on p.ProductTypeId = pt.ProductTypeId
		LEFT OUTER JOIN Enterprise.ProductType PT2 on PT.ParentProductTypeId = PT2.ProductTypeId
		INNER JOIN @CompanyOrganizationProduct OP on P.ProductId = OP.ProductId
		LEFT OUTER JOIN ProductSettings ps1 on ps1.ProductId = p.ProductId and ps1.name = 'IsNewTab'
		LEFT OUTER JOIN ProductSettings ps2 on ps2.ProductId = p.ProductId and ps2.name = 'IsResource'
		LEFT OUTER JOIN ProductSettings ps3 on ps3.ProductId = p.ProductId and ps3.name = 'ProductUrl'
		LEFT OUTER JOIN ProductSettings ps4 on ps4.ProductId = p.ProductId and ps4.name = 'ShowInAppSwitcher'
	WHERE 
		ppv.PersonaId = @personaid
		
	ORDER BY IsResource, FamilyName, P.Name
END
--select ps.productid, pst.name, ps.value from enterprise.ProductSetting ps inner join enterprise.ProductSettingType pst on ps.ProductSettingTypeId = pst.ProductSettingTypeId where pst.name in ( 'isresource', 'isnewtab' )

--select * from enterprise.product p inner join enterprise.ProductType pt on p.ProductTypeId = pt.ProductTypeId where productid = 29
--select * from enterprise.product where productid = 28
/*
select * from enterprise.ProductType
select * from enterprise.productsetting ps inner join enterprise.ProductSettingType pst on ps.ProductSettingTypeId = pst.ProductSettingTypeId
where ps.ProductId = 29

and pst.name in ( '
*/
/*
--"id": 29,
--"name": "Business Intelligence",
"url": "https://www-sat.realpage.com/home/product-redirect.html?prod=29&persona=21260",
"description": "Business Intelligence provides portfolio reporting spanning multiple source systems which serves up key business metrics with a front end business analytics tool.  Data has been normalized into business models to improve reporting quality.",
"label": "business-intelligence",
--"familyId": 400,
"familyName": "Asset Optimization",
"isNewTab": true,
--"isFavorite": false,
"isResource": false,
--"status": 8,
--"productCode": "BI"
*/
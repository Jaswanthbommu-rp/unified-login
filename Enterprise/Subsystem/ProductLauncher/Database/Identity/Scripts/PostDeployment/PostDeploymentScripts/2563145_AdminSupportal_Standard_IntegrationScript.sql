--User Story 2563145: Product Integration - Admin & Support Portal (Transition to Standard Integration) - DEV Only

DECLARE @ProductsettingTypeid int;
DECLARE @UserId bigint, @RightId bigint, @OrgPartyId bigint

IF NOT EXISTS (SELECT * FROM Enterprise.ProductSettingType WHERE [Name] = 'SuperUserRoleType')
BEGIN
    INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
    VALUES ('SuperUserRoleType', 'Super User Role Type', 0);
END
GO


DECLARE @UserId bigint
SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS (SELECT TOP 1 1 FROM security.[Right] where Rightname = 'ManageAdminSupportPortalProductAccess')
BEGIN
	INSERT INTO security.[Right] (RightName	,[Description],	[Value],	StatusTypeId,	VisibilityStatusId,	ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation)
	VALUES('ManageAdminSupportPortalProductAccess','For Admin & Support Portal, this right unlocks the ability to edit the Product assignment and Product Access Details for a user, assuming that the user can access the page because of Ability to view users.  Al',	'Manage AdminSupportPortal Product Access',13,9,3,104,@UserId,GETUTCDATE(),0,0)
	
END
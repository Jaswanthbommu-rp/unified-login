

-- Add the EnterpriseRole right
 DECLARE @RightValue nvarchar(200),
		 @UserId bigint,
		 @Now datetime = GETDATE(),
		 @RightId int,
		 @RoleId INT,
		 @ProductId int = 3,
		 @TargetProductId int = 3,
		 @RoleName nvarchar(100),
		 @OrgVisibilityStatusId INT = 9,
		 @RightVisibilityStatusId INT =10,
		 @StatusTypeId int=13;
SELECT	@UserId = UserId
	FROM	Ident.UserLogin
	WHERE	LoginName LIKE 'realpagead@%'
IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='EnterpriseRole')
BEGIN 
		INSERT INTO Security.[Right] (RightName,[Description],[Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
		VALUES('EnterpriseRole','Manage Enterprise Roles','Ability to view, edit and create Enterprise Roles',@StatusTypeId,@RightVisibilityStatusId,@ProductId ,@TargetProductId,@UserId,@Now);
END
SELECT @RoleId = RoleId from [Security].[Role] where RoleName='User Administrator' And OrgPartyID IS NULL;
SELECT @RightId =  RightId from [Security].[Right] where RightName = 'EnterpriseRole';
IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[RoleRight] WHERE [RightId]= @RightId)
BEGIN
	INSERT INTO Security.RoleRight (RoleId,RightId,CreatedBy,CreatedDate) 
	VALUES(@RoleId,@RightId,@UserId,@Now);
END

-----------------------End of Enterprise Role creation.




IF EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='PrimaryPropertyEnterpriseRole')
BEGIN
update [Security].[Right] set RightName = 'PrimaryProperty' , [Description] = 'Manage Primary Properties', [Value] = 'Ability to view, edit and create Primary Properties' WHERE RightName ='PrimaryPropertyEnterpriseRole'
-- Inserting PrimaryProperty as Mapping name by taking created and updated date as same as PrimaryPropertyEnterpriseRole.
insert into Settings.OrganizationSettings(PartyId, SettingCategoryTypeId, MappingName, MappingValue, Editable, [Hidden], CreatedBy, CreatedDate, UpdatedDate)
select PartyId, SettingCategoryTypeId, 'PrimaryProperty', MappingValue, Editable, [Hidden], CreatedBy, CreatedDate, UpdatedDate from Settings.OrganizationSettings where MappingName ='PrimaryPropertyEnterpriseRole'

update Settings.OrganizationSettings set MappingName = 'EnterpriseRole' where  MappingName ='PrimaryPropertyEnterpriseRole'
update Enterprise.NavigationMenuSettingAccess set MappingName = 'EnterpriseRole' where MappingName = 'PrimaryPropertyEnterpriseRole'
END

















declare @UserId bigint
 SELECT @UserId = UserId
       FROM   Ident.UserLogin
       WHERE  LoginName LIKE 'realpagead@%'

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right]  where [RightName] = 'ManageKnockProductaccess' )
BEGIN 
declare @RightId int = 0
insert into [Security].[Right] 
values('ManageKnockProductaccess','Manage Knock Product access','Manage Knock Product access',13,9,3,3,@UserId,getdate(),0)

select @RightId = RightId from [Security].[Right] where [RightName] = 'ManageKnockProductaccess'

insert into [Security].[RoleRight] values (1,@RightId,@UserId,getdate())

END


GO

--Bug 1542068: PME-335178 - Re-assigning Knock CRM product to Unity user does not reactivate the user in Knock
IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'IsActivateUserBeforeUpdate')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData) 
	VALUES('IsActivateUserBeforeUpdate', 'Deactivated user should be activated before updating user (patch call)', 0)
END
GO
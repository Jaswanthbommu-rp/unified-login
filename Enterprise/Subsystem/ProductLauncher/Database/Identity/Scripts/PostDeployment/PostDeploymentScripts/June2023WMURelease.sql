

Declare @Id int, @rightId bigint;
Select @Id = Id from Enterprise.NavigationMenu where Title = 'Manage Reports' and Origin = 'unified-reporting';
Select @rightId = RightId from Security.[Right] where RightName = 'ManagePropertyLevelReporting';
IF NOT EXISTS (Select Top 1 1 from enterprise.navigationMenuRights where NavigationMenuId =@Id and RightId = @rightId)
BEGIN
  Insert into enterprise.navigationMenuRights (NavigationMenuId,RightId) values (@Id,@rightId);
END
GO


--Bug 1542068: PME-335178 - Re-assigning Knock CRM product to Unity user does not reactivate the user in Knock
IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'IsActivateUserBeforeUpdate')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData) 
	VALUES('IsActivateUserBeforeUpdate', 'Deactivated user should be activated before updating user (patch call)', 0)
END
GO

Declare @RightId bigint,@RoleID bigint,@UserId bigint;
SELECT	@UserId = UserId
	FROM	Ident.UserLogin
	WHERE	LoginName LIKE 'realpagead@%'
If Not Exists (Select Top 1 1 from Security.[Right] where RightName = 'ManageCommunityRewardsProductAccess')
Begin
insert into Security.[Right] values ('ManageCommunityRewardsProductAccess', 'Manage Community Rewards Product Access','Manage Community Rewards Product Access',13,9,3,77,6357,GETDATE())
END
Select @RightId = RightId from Security.[Right] where RightName = 'ManageCommunityRewardsProductAccess';
Select @RoleID = RoleID from Security.Role where RoleName = 'User Administrator';
If Not Exists (Select Top 1 1 from Security.RoleRight where RoleId =@RoleID and RightID = @RightId)
Begin
 Insert into Security.RoleRight values (@RoleID,@RightId,@UserId,GETDATE())
End
GO
--ProductAsideInfoData
if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'ProductAsideInfoData' )
begin
	insert into enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'ProductAsideInfoData', 'The type of data which loads aside info grid.For example groupproperties or rights.', 0)
end
GO
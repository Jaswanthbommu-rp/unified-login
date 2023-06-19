GO

Declare @Id int, @rightId bigint;
Select @Id = Id from Enterprise.NavigationMenu where Title = 'Manage Reports' and Origin = 'unified-reporting';
Select @rightId = RightId from Security.[Right] where RightName = 'ManagePropertyLevelReporting';

IF NOT EXISTS (Select Top 1 1 from enterprise.navigationMenuRights where NavigationMenuId =@Id and RightId = @rightId)
BEGIN
  Insert into enterprise.navigationMenuRights (NavigationMenuId,RightId) values (@Id,@rightId);
END

Go
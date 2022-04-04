
--1019723

Declare @NavigationMenuID BIGINT, @RightID BIGINT;
Select @RightID = RightId from Security.[Right] where value = 'Employee Access to Manage Settings Templates';
Select @NavigationMenuID = Id  from Enterprise.NavigationMenu where Title = 'Manage Templates' and PageId ='manage-templates' and Origin ='unified-settings';
IF NOT EXISTS (Select Top 1 1 from ENterprise.NavigationMenuRights where NavigationMenuId =@NavigationMenuID and RightId = @RightID)
BEGIN
INSERT INTO ENterprise.NavigationMenuRights (NavigationMenuId,RightId)
                                 Values (@NavigationMenuID,@RightID);
END

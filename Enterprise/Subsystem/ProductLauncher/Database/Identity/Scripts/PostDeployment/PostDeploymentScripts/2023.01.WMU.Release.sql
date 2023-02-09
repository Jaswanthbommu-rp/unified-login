

Declare @UserId bigint;
SELECT @UserId = UserId
FROM   Ident.UserLogin
WHERE  LoginName LIKE 'realpagead@%';

-------------------Role--------------------------------------

IF NOT EXISTS (Select Top 1 1 from Security.Role where RoleName = 'Basic' and ProductId = 90)
Begin
    Insert into Security.Role values ('Basic','Basic','Basic',3,Null,90,@UserId,GETUTCDATE());
End
IF NOT EXISTS (Select Top 1 1 from Security.Role where RoleName = 'Admin' and ProductId = 90)
Begin
    Insert into Security.Role values ('Admin','Admin','Admin',3,Null,90,@UserId,GETUTCDATE());
End
IF NOT EXISTS (Select Top 1 1 from Security.Role where RoleName = 'OneSite' and ProductId = 90)
Begin
    Insert into Security.Role values ('OneSite','OneSite','OneSite',3,Null,90,@UserId,GETUTCDATE());
End
---------------------Right--------------------------------

IF NOT EXISTS (Select Top 1 1 from security.[Right] where RightName = 'Basic' and ProductId = 90)
Begin
   Insert into Security.[Right] values ('Basic','Basic','Basic',13,9,90,90,@UserId,GETUTCDATE(),0);
End
IF NOT EXISTS (Select Top 1 1 from security.[Right] where RightName = 'Admin' and ProductId = 90)
Begin
   Insert into Security.[Right] values ('Admin','Admin','Admin',13,9,90,90,@UserId,GETUTCDATE(),0);
End
IF NOT EXISTS (Select Top 1 1 from security.[Right] where RightName = 'OneSite' and ProductId = 90)
Begin
   Insert into Security.[Right] values ('OneSite','OneSite','OneSite',13,9,90,90,@UserId,GETUTCDATE(),0);
End

--------------RoleRight----------------------------

Declare @right1 BigInt,@right2 BigInt,@right3 BigInt,@role1 Bigint,@role2 BigInt, @role3 Bigint;

Select @role1 = RoleId from Security.[Role] where RoleName = 'Basic' and ProductId = 90;
Select @role2 = RoleId from Security.[Role] where RoleName = 'Admin' and ProductId = 90;
Select @role3 = RoleId from Security.[Role] where RoleName = 'OneSite' and ProductId = 90;

Select @right1 = RightId from Security.[Right] where RightName = 'Basic' and ProductId = 90;
Select @right2 = RightId from Security.[Right] where RightName = 'Admin' and ProductId = 90;
Select @right3 = RightId from Security.[Right] where RightName = 'OneSite' and ProductId = 90;


If not Exists (Select Top 1 1 from Security.[RoleRight] where RightId = @right1 and RoleId = @role1)
Begin
   Insert into Security.[RoleRight] values (@role1,@right1,@UserId,GETUTCDATE())
End
If not Exists (Select Top 1 1 from Security.[RoleRight] where RightId = @right2 and RoleId = @role2)
Begin
   Insert into Security.[RoleRight] values (@role2,@right2,@UserId,GETUTCDATE())
End
If not Exists (Select Top 1 1 from Security.[RoleRight] where RightId = @right3 and RoleId = @role3)
Begin
   Insert into Security.[RoleRight] values (@role3,@right3,@UserId,GETUTCDATE())
End

IF not Exists (Select Top 1 1 from Security.[Right] where RightName = 'ManageDataHubProductaccess' and ProductId =3 and TargetProductId =90)
BEGIN
    Insert into Security.[Right] values ('ManageDataHubProductaccess','Manage DataHub Product access','Manage DataHub Product access',13,9,3,90,@UserId,GETUTCDATE(),0);
END

 Declare @rrRightId BigInt;
Select @rrRightId = RightId from Security.[Right] where RightName = 'ManageDataHubProductaccess';

IF not Exists (Select Top 1 1 from Security.[RoleRight] where RoleId = 1 and RightId = @rrRightId)
Begin 
   Insert into Security.[RoleRight] values (1,@rrRightId,@UserId,GETUTCDATE());
end
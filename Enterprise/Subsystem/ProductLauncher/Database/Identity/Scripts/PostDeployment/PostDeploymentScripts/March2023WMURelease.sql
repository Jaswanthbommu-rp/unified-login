Declare @UserId bigint;
SELECT @UserId = UserId
FROM   Ident.UserLogin
WHERE  LoginName LIKE 'realpagead@%';


IF NOT EXISTS (Select Top 1 1 from security.role where productId =92)
BEGIN
     Insert into security.role values ('Sustainability Analyst','SustainabilityAnalyst','Sustainability Analyst',3,null,92,@UserId,GETUTCDATE());
END

IF NOT EXISTS (Select Top 1 1 from  Security.[Right] where RightName = 'ManageSustainabilityAnalystProductaccess')
Begin
    Insert into Security.[Right] values ('ManageSustainabilityAnalystProductaccess','Manage Sustainability Analyst Product access','Manage Sustainability Analyst Product access',13,9,3,3,@UserId,GETUTCDATE(),1);
end

Declare @rightId bigint;
Select @rightId = RightId from Security.[Right] where RightName = 'ManageSustainabilityAnalystProductaccess';

IF Not Exists (Select Top 1 1 from Security.RoleRight where RoleId = 1 and RightId =@rightId)
Begin
   Insert into security.RoleRight values (1,@rightId,@UserId,GETUTCDATE())
End

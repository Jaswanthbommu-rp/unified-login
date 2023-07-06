
IF NOT EXISTS (Select TOP 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName in ('Operator (External User)'))
BEGIN
  Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],[PartyRoleTypeId],[ThirdPartyRelationshipId]) 
         values (1,50,'Operator (External User)','Non-Employee user that is a Fee Manager',405,1);
END

IF NOT EXISTS (Select TOP 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName in ('Owner (External User)'))
BEGIN
  Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],[PartyRoleTypeId],[ThirdPartyRelationshipId]) 
         values (2,60,'Owner (External User)','Non-Employee user that is a portfolio owner',405,2);
END

IF NOT EXISTS (Select TOP 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName in ('Third Party Vendor (External User)'))
BEGIN
   Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],[PartyRoleTypeId],[ThirdPartyRelationshipId]) 
         values (3,70,'Third Party Vendor (External User)','Non-Employee user that is a Third Party Vendor',405,3);
END

IF NOT EXISTS (Select TOP 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName in ('Employee'))
BEGIN
   Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],[PartyRoleTypeId],[ThirdPartyRelationshipId]) 
         values (4,10,'Employee','Employee of my company',401,4);
END

IF NOT EXISTS (Select TOP 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName in ('Other (External User)'))
BEGIN
   Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],[PartyRoleTypeId],[ThirdPartyRelationshipId]) 
         values (5,80,'Other (External User)','Non-Employee user with other relationship',405,5);
END

IF NOT EXISTS (Select TOP 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName in ('Employee (No Email)'))
BEGIN
   Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],[PartyRoleTypeId],[ThirdPartyRelationshipId]) 
         values (6,20,'Employee (No Email)','Employee of my company with non-email username',404,4);
END

IF NOT EXISTS (Select TOP 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName in ('Employee (Additional Company)'))
BEGIN
   Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],[PartyRoleTypeId],[ThirdPartyRelationshipId]) 
         values (7,30,'Employee (Additional Company)','Employee user that needs additional company access',405,4);
END
		 
IF NOT EXISTS (Select TOP 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName in ('System Administrator'))
BEGIN
     Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],[PartyRoleTypeId],[ThirdPartyRelationshipId]) 
         values (8,40,'System Administrator','Employee Super User can manage users and  access all products',402,4);
END

	
IF NOT EXISTS (Select TOP 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName in ('RealPage Employee'))
BEGIN
     Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],[PartyRoleTypeId],[ThirdPartyRelationshipId]) 
         values (9,90,'RealPage Employee','Employee for RealPage',403,4);
END	
 
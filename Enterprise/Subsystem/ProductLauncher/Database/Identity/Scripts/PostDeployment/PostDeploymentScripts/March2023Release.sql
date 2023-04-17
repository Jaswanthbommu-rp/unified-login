IF NOT EXISTS (Select Top 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName = 'Employee')  
BEGIN
  Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],PartyRoleTypeId,ThirdPartyRelationshipId)
                                      values (1,10,'Employee','Employee user with email format username',401,4);
END
IF NOT EXISTS (Select Top 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName = 'Employee (no email)')  
BEGIN
  Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],PartyRoleTypeId,ThirdPartyRelationshipId)
                                      values (2,20,'Employee (no email)','Employee user with non-email username',404,4);
END
IF NOT EXISTS (Select Top 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName = 'Employee (additional company)')  
BEGIN
   Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],PartyRoleTypeId,ThirdPartyRelationshipId)
                                      values (3,30,'Employee (additional company)','Employee user that needs additional company access',405,4);
END

IF NOT EXISTS (Select Top 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName = 'System Administrator')  
BEGIN
   Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],PartyRoleTypeId,ThirdPartyRelationshipId)
                                      values (4,40,'System Administrator','Company Super User',402,4);
END

IF NOT EXISTS (Select Top 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName = 'Operator (External User)')  
BEGIN
   Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],PartyRoleTypeId,ThirdPartyRelationshipId)
                                      values (5,50,'Operator (External User)','Non-Employee user that is a Fee Manager',405,1);
END

IF NOT EXISTS (Select Top 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName = 'Owner (External User)')  
BEGIN
   Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],PartyRoleTypeId,ThirdPartyRelationshipId)
                                      values (6,60,'Owner (External User)','Non-Employee user that is a Portfolio Owner',405,2);
END
IF NOT EXISTS (Select Top 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName = 'Vendor (External User)')  
BEGIN
  Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],PartyRoleTypeId,ThirdPartyRelationshipId)
                                      values (7,70,'Vendor (External User)','Non-Employee user that is a 3rd Party Vendor',405,3);
END
IF NOT EXISTS (Select Top 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName = 'RealPage Employee')  
BEGIN
  Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],PartyRoleTypeId,ThirdPartyRelationshipId)
                                      values (8,80,'RealPage Employee','',403,4);
END
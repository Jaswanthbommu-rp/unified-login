
IF NOT EXISTS (Select Top 1 1 from [Enterprise].[ThirdPartyRelationship] where ThirdPartyRelationship = 'Operator (External User - No Email)')
BEGIN
  INSERT [Enterprise].[ThirdPartyRelationship] ([ThirdPartyRelationshipId], [ThirdPartyRelationship]) VALUES (10, N'Operator (External User - No Email)')
END
IF NOT EXISTS (Select Top 1 1 from [Enterprise].[ThirdPartyRelationship] where ThirdPartyRelationship = 'Other (External User - No Email)')
BEGIN
  INSERT [Enterprise].[ThirdPartyRelationship] ([ThirdPartyRelationshipId], [ThirdPartyRelationship]) VALUES (11, N'Other (External User - No Email)')
END

IF NOT EXISTS (Select Top 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName = 'Operator (External User - No Email)')  
BEGIN
  Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],PartyRoleTypeId,ThirdPartyRelationshipId)
                                      values (10,51,'Operator (External User - No Email)','Non-Employee user that is a Fee Manager with no-email',404,10);
END
IF NOT EXISTS (Select Top 1 1 from [Enterprise].[UserRelationShip] where UserRelationshipName = 'Operator (External User - No Email)')  
BEGIN
  Insert into [Enterprise].[UserRelationShip] (Id,SortIndex,UserRelationshipName,[Description],PartyRoleTypeId,ThirdPartyRelationshipId)
                                      values (11,81,'Other (External User - No Email)','Non-Employee user with no-email',404,11);
END


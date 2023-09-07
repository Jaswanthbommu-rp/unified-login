 
 

 IF NOT EXISTS( select top 1 1 from [Security].[Role] where RoleName = 'Sustainability Admin' )
 BEGIN
 declare @orgPartyId bigint = (select top 1 PartyId from Enterprise.Organization where [Name] like '%RealPage Employee%')
 declare @UserId nvarchar(20) =(select UserId FROM	Ident.UserLogin WHERE	LoginName LIKE 'realpagead@%')

 insert into [Security].[Role] (RoleName,ShortName,[Description],RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
 values ('Sustainability Admin','SustainabilityAdmin','Sustainability Admin',3,@orgPartyId,92,@UserId,GETUTCDATE())
    
	insert into [Security].[Right] (RightName,[Description],[Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight)
	 values ('SustainabilityAdmin','Sustainability Admin','SustainabilityAdmin',13,	9,	3,	92,	@UserId,	GETUTCDATE(),	0)

	 declare @rightId int = (select top 1 RightId from [Security].[Right] where RightName = 'SustainabilityAdmin')
     declare @roleId int = (select top 1  RoleId from [Security].[Role] where RoleName = 'Sustainability Admin') 

	 insert into [Security].RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	 values(@roleId,@rightId,@UserId,GETUTCDATE())

 END




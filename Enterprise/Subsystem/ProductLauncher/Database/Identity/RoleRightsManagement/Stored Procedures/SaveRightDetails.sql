CREATE PROCEDURE [Security].[SaveRightDetails] (
	@JsonRightsData nvarchar(max),
	@ModifiedBy bigint)
AS
BEGIN  
 BEGIN TRY  
  Declare @SecurityRight Table(  
   [RightId] INT ,  
   [RightName] [nvarchar](100) ,  
   [Description] [nvarchar](255) ,  
   [Value] [nvarchar](255) ,  
   [StatusTypeId] [int] ,  
   [VisibilityStatusId] [int] ,  
   [ProductId] [int] ,  
   [TargetProductId] [int],
   [IsExcludeRightFromImpersonation] [bit],
   [PersistRight] [bit])  
  
 Declare @RightRoute Table(  
  RightId INT ,  
  RouteId INT)  
  
 Declare @RightRole Table(  
  RightId INT ,  
  RoleId INT)  
  
 Declare @OverRideRight Table(  
  RightId INT ,  
  OrgPartyId BIGINT)  
  
  --Right Details  
  Insert Into @SecurityRight  
  Select RightsData.*  
  FROM OPENJSON (@JsonRightsData, N'$')  
    WITH (  
   RightId INT N'$.Detail.RightId',  
   RightName Varchar(100) N'$.Detail.RightName',  
   [Description] varchar(255) N'$.Detail.Description',  
   [Value] varchar(255) N'$.Detail.Value',  
   [StatusTypeId] INT N'$.Detail.StatusTypeId',  
   [VisibilityStatusId] INT N'$.Detail.VisibilityStatusId',  
   [ProductId] INT N'$.Detail.ProductId',  
   [TargetProductId] INT N'$.Detail.TargetProductId',
   [IsExcludeRightFromImpersonation] bit N'$.Detail.IsExcludeRightFromImpersonation',
   [PersistRight] bit N'$.Detail.IsExcludeRightFromImpersonation'
    ) AS RightsData;  
  
  --Right Route  
  Insert Into @RightRoute  
  Select i.RightId,r.RouteId  
  FROM OPENJSON (@JsonRightsData, N'$')  
   WITH (  
   RightId INT N'$.Detail.RightId',  
   RouteData nvarchar(max) '$.Routes' AS JSON  
    ) AS i  
    CROSS APPLY OPENJSON(i.RouteData)  
    WITH(RouteId INT '$.RouteId') r;  
     
   --Override Right  
   Insert Into @OverRideRight  
   Select i.RightId,r.OrgPartyId  
   FROM OPENJSON (@JsonRightsData, N'$')  
   WITH (  
   RightId INT N'$.Detail.RightId',  
   ExcludedCompaniesData nvarchar(max) '$.ExcludedCompanies' AS JSON  
    ) AS i  
    CROSS APPLY OPENJSON(i.ExcludedCompaniesData)  
    WITH(OrgPartyId BIGINT '$.OrganizationId') r;  
  
    --Right role  
    Insert Into @RightRole  
    Select i.RightId,r.RoleId  
    FROM OPENJSON (@JsonRightsData, N'$')  
    WITH (  
   RightId INT N'$.Detail.RightId',  
   RoleData nvarchar(max) '$.Roles' AS JSON  
    ) AS i  
    CROSS APPLY OPENJSON(i.RoleData)  
    WITH(RoleId INT '$.RoleId') r;  
  
    Declare @RightId int,@VisibilityStatusId Int, @RightName varchar(100);  
    Select @RightId = ISNULL(RightId,0), @RightName = RightName From @SecurityRight  
  
    IF (@RightId = 0)  
    BEGIN  
   INSERT INTO Security.[Right](RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,IsExcludeRightFromImpersonation,PersistRight)  
   Select RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,@ModifiedBy,GETUTCDATE(),IsExcludeRightFromImpersonation,PersistRight 
   From @SecurityRight  
   SET @RightId = SCOPE_IDENTITY();  
  
   IF (Select Count(*) From @RightRole) > 0  
   Begin  
    INSERT INTO Security.RoleRight(RightId,RoleId,CreatedBy,CreatedDate)  
    Select @RightId,RoleId,@ModifiedBy,GETUTCDATE() From @RightRole  
   End  
  
   IF (Select Count(*) From @RightRoute) > 0  
   Begin  
    INSERT INTO Security.RightRoute(RightId,RouteId,CreatedBy,CreatedDate)  
    Select @RightId,RouteId, @ModifiedBy,GETUTCDATE() From @RightRoute  
   End  
  
   IF (Select Count(*) From @OverRideRight) > 0  
   Begin  
    INSERT INTO Security.OrganizationOverRideRight(RightId,OrgPartyId,VisibilityStatusId,CreatedBy,CreatedDate)  
    Select @RightId,OrgPartyId,9,@ModifiedBy,GETUTCDATE() From @OverRideRight  
   End  
    END  
    ELSE  
    BEGIN  
   IF (Select Count(*) From @SecurityRight) > 0  
   Begin       
    Update R Set RightName = SR.RightName,  
        Description = SR.Description,  
        Value = SR.Value,  
        StatusTypeId = sr.StatusTypeId,  
        VisibilityStatusId = SR.VisibilityStatusId,  
        ProductId = SR.ProductId,  
        TargetProductId = SR.TargetProductId, 
		IsExcludeRightFromImpersonation =SR.IsExcludeRightFromImpersonation, 
        PersistRight = SR.PersistRight,
        CreatedBy = @ModifiedBy,  
        CreatedDate = GETUTCDATE()  
    From Security.[Right] R  
    Join @SecurityRight SR ON R.RightId = SR.RightId  
  
    --Delete and re insert data based on incoming data, if ui send empty object it will delete all data  
    Delete From Security.RoleRight Where RightId = @RightId  
    IF (Select Count(*) From @RightRole) > 0  
    Begin  
     INSERT INTO Security.RoleRight(RightId,RoleId,CreatedBy,CreatedDate)  
     Select RightId,RoleId,@ModifiedBy,GETUTCDATE() From @RightRole  
    End  
  
    Delete From Security.RightRoute Where RightId = @RightId  
    IF (Select Count(*) From @RightRoute) > 0  
    Begin  
     INSERT INTO Security.RightRoute(RightId,RouteId,CreatedBy,CreatedDate)  
     Select @RightId,RouteId, @ModifiedBy,GETUTCDATE() From @RightRoute  
    End  
  
    Delete From Security.OrganizationOverRideRight Where RightId = @RightId  
    IF (Select Count(*) From @OverRideRight) > 0  
    Begin  
     INSERT INTO Security.OrganizationOverRideRight(RightId,OrgPartyId,VisibilityStatusId,CreatedBy,CreatedDate)  
     Select RightId,OrgPartyId,9,@ModifiedBy,GETUTCDATE() From @OverRideRight  
    End  
   End  
    END  
    
  SELECT @RightId AS Id,'' AS ErrorMessage  
    
 END TRY  
 BEGIN CATCH  
        DECLARE @ErrorLogID int;  
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;  
  
        SELECT 0 AS Id,  
     ErrorMessage  
        FROM dbo.ErrorLog  
        WHERE ErrorLogID = @ErrorLogID;  
 END CATCH  
END
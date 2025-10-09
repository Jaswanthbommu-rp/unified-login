CREATE PROCEDURE Ident.CreateUserLoginPersona (  
 @UserLoginId bigint  
 ,@StatusTypeId int  
 ,@OrganizationPartyId bigint  
 ,@Primaryorganization bit  
 ,@Fromdate datetime  
 ,@ThruDate datetime = null  
 ,@StatusThruDate datetime = null  
 ,@IsRPEmployee bit = 0  
 ,@IsRealPartner bit = 0
 ,@IsDelegateAdmin bit =0  
 )  
AS  
BEGIN  
 IF @FromDate IS NULL  
 BEGIN  
  SELECT @FromDate = GETUTCDATE()  
 END;  
  
 BEGIN TRANSACTION;   
  
 BEGIN TRY  
  INSERT INTO [Ident].[UserLoginPersona] (  
   [UserLoginId]  
   ,[StatusTypeId]  
   ,[OrganizationPartyId]  
   ,[PrimaryOrganization]  
   ,[FromDate]  
   ,[ThruDate]  
   ,[StatusThruDate]  
   ,[IsRPEmployee]  
   ,[IsRealPartner]
   ,[IsDelegateAdmin]  
   ,[LastLoginDate]
   ,[UserDeactivationDate]
  )  
  VALUES (  
   @UserLoginId  
   ,@StatusTypeId  
   ,@OrganizationPartyId  
   ,@PrimaryOrganization  
   ,@FromDate  
   ,@ThruDate  
   ,@StatusThruDate  
   ,@IsRPEmployee  
   ,@IsRealPartner
   ,@IsDelegateAdmin  
   ,NULL
   ,NULL
  )  
  SELECT SCOPE_IDENTITY() AS Id,  
     '' AS ErrorMessage;  
  
  UPDATE Ident.UserLoginPersona  
  SET   PrimaryOrganization = 'False',  
     StatusTypeId = case when StatusTypeId = 2 then 1 else StatusTypeId end,  
     StatusThruDate = case when StatusTypeId = 2 then null else StatusThruDate END,
	 UserDeactivationDate = CASE WHEN @StatusTypeId = 24 THEN GETUTCDATE() ELSE NULL END
  WHERE UserLoginId = @UserLoginId  
  AND   PrimaryOrganization = 'True'  
  AND   OrganizationPartyId != @OrganizationPartyId  
  AND   @primaryorganization = 'True'  
  
  COMMIT;  
 END TRY  
 BEGIN CATCH  
  ROLLBACK;  
  DECLARE @ErrorLogID int;  
  EXEC dbo.LogError  
   @ErrorLogID = @ErrorLogID OUTPUT;  
  SELECT 0 AS Id  
      ,'' AS RealPageId,  
      ErrorMessage  
  FROM  dbo.ErrorLog  
  WHERE ErrorLogID = @ErrorLogID;  
 END CATCH;  
END
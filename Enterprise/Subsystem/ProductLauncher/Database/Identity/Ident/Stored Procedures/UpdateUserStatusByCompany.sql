CREATE PROCEDURE [Ident].[UpdateUserStatusByCompany] (  
 @RealPageId uniqueidentifier ,   
 @OrganizationPartyId int,  
 @StatusTypeId int,  
 @FromDate datetime,  
 @StatusThruDate datetime = NULL  
)  
AS  
BEGIN  
 DECLARE @ErrorLogID int,  
  @UserId bigint,  
  @CurrentUserStatusId INT,  
  @UserLastLoginDate DateTime = Null,  
  @UserLoginPersonaId INT,  
  @PrimaryOrganization TINYINT,  
  @NOW datetime = GETUTCDATE();  
  
 BEGIN TRY  
  SELECT @UserId =  UL.UserId,  
      @CurrentUserStatusId = ULP.StatusTypeId,  
      @UserLastLoginDate = UL.LastLoginDate,  
      @UserLoginPersonaId = ULP.UserLoginPersonaId,  
      @PrimaryOrganization = ULP.PrimaryOrganization  
  FROM  Ident.UserLogin UL  
      INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId  
      INNER JOIN  Enterprise.Party P ON P.PartyId = UL.PersonPartyId  
  WHERE   
   P.RealPageId = @RealPageId  
   AND   
   ULP.OrganizationPartyId = @OrganizationPartyId  
  
  IF (@CurrentUserStatusId = 24 And (@StatusTypeId = 1 OR @StatusTypeId = 2))  
  BEGIN  
   UPDATE Ident.UserLoginPersona  
   SET   StatusTypeId = @StatusTypeId,  
      FromDate = @NOW,  
      StatusThruDate =  @StatusThruDate,  
      ThruDate = NULL,
	  UserDeactivationDate =  CASE WHEN @StatusTypeId = 24 AND @FromDate < @NOW THEN GETUTCDATE() ELSE NULL END
   WHERE UserLoginPersonaId = @UserLoginPersonaId  
  END  
  ELSE  
  BEGIN  
   UPDATE Ident.UserLoginPersona  
   SET   StatusTypeId = @StatusTypeId,  
		 StatusThruDate =  ISNULL(@StatusThruDate, Null),
		 UserDeactivationDate =  CASE WHEN @StatusTypeId = 24 AND @FromDate < @NOW THEN GETUTCDATE() ELSE NULL END
   WHERE   
    UserLoginPersonaId = @UserLoginPersonaId  
  
   if @StatusTypeId = 24 AND @PrimaryOrganization = 1 AND (SELECT Count(1) FROM Ident.UserLoginPersona WHERE PrimaryOrganization = 0 AND UserLoginId = @UserId AND StatusTypeId != 24) > 0  
   BEGIN  
    -- CHECK FOR ADDITIONAL COMPANIES AND DISABLE THEM IF THIS IS THE USERS PRIMARY COMPANY  
    UPDATE Ident.UserLoginPersona  
    SET   StatusTypeId = @StatusTypeId,  
          StatusThruDate =  ISNULL(@StatusThruDate, Null),
		  UserDeactivationDate =  CASE WHEN @StatusTypeId = 24 AND @FromDate < @NOW THEN GETUTCDATE() ELSE NULL END
    WHERE   
     UserLoginId = @UserId  
     AND  
     PrimaryOrganization = 0  
     AND  
     StatusTypeId != 24  
   END  
  END  
  
  SELECT @UserId AS Id,  
      '' AS ErrorMessage  
 END TRY  
 BEGIN CATCH  
  EXEC dbo.LogError  
   @ErrorLogID = @ErrorLogID OUTPUT;  
  
   SELECT 0 AS Id,  
       ErrorMessage  
   FROM  dbo.ErrorLog  
   WHERE ErrorLogID = @ErrorLogID;  
 END CATCH   
END
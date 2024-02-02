CREATE PROCEDURE [Ident].[UpdateUserStatus] (    
 @StatusTypeId int,    
 @FromDate datetime,    
 @StatusThruDate datetime = NULL,    
 @ThruDate datetime = NULL,    
 @IsMultiCompanyUser BIT,    
 @UserLoginPersonaId INT,    
 @UserId bigint,    
 @PrimaryOrganization TINYINT    
)    
AS    
BEGIN    
 UPDATE Ident.UserLoginPersona    
 SET    StatusTypeId = @StatusTypeId,    
		FromDate = IsNull(@FromDate,FromDate),    
		StatusThruDate =  ISNULL(@StatusThruDate, NULL),    
		ThruDate = ISNULL(@ThruDate, ThruDate),
		UserDeactivationDate =  CASE WHEN @StatusTypeId = 24 THEN GETUTCDATE() ELSE NULL END
 WHERE UserLoginPersonaId = @UserLoginPersonaId    
 --If we are trying to disable the userStatus and we are trying to do it in the primary company, while he is not disabled in the secondary companies    
 if @StatusTypeId = 24 AND @PrimaryOrganization = 1 AND @IsMultiCompanyUser = 1    
 BEGIN    
  -- CHECK FOR ADDITIONAL COMPANIES AND DISABLE THEM IF THIS IS THE USERS PRIMARY COMPANY    
  UPDATE Ident.UserLoginPersona    
  SET   StatusTypeId = @StatusTypeId,    
		StatusThruDate =  ISNULL(@StatusThruDate, NULL),
		UserDeactivationDate =  CASE WHEN @StatusTypeId = 24 THEN GETUTCDATE() ELSE NULL END
  WHERE     
   UserLoginId = @UserId    
   AND    
   PrimaryOrganization = 0    
   AND    
   StatusTypeId != 24    
 END    
END
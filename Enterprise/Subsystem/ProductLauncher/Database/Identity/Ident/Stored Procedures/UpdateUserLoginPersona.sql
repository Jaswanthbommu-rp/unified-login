CREATE PROCEDURE Ident.UpdateUserLoginPersona (  
 @UserLoginId bigint,  
 @StatusTypeId int,  
 @OrganizationPartyId bigint,  
 @Primaryorganization bit,  
 @Fromdate datetime = NULL,  
 @ThruDate datetime = NULL,  
 @StatusThruDate datetime = NULL  
 )  
AS  
BEGIN  
 BEGIN TRANSACTION;   
  
 BEGIN TRY  
  UPDATE [Ident].[UserLoginPersona]  
	SET
		StatusTypeId = @StatusTypeId,  
		[PrimaryOrganization] = @Primaryorganization,  
		Fromdate = ISNULL(@Fromdate, Fromdate),  
		ThruDate = ISNULL(@ThruDate, ThruDate),  
		StatusThruDate = ISNULL(@StatusThruDate, StatusThruDate),
		UserDeactivationDate =  CASE WHEN @StatusTypeId = 24 THEN GETUTCDATE() ELSE NULL END
  OUTPUT inserted.UserLoginPersonaId AS Id,  
      '' AS ErrorMessage  
  WHERE UserLoginId = @UserLoginId  
  AND   OrganizationPartyId = @OrganizationPartyId  
  
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
CREATE PROCEDURE [Enterprise].[UpdateExternalUserRelationship]
(
	@UserLoginPersonaId BIGINT,
	@ThirdPartyRelationshipId TINYINT,
	@CompanyName VARCHAR(200) = NULL,
	@ThirdPartyCompanyRealPageId UNIQUEIDENTIFIER = NULL,    
    @OperatorCode VARCHAR(250) = NULL,
    @OperatorValue VARCHAR(500) = NULL
)
AS
BEGIN
	DECLARE @CompanyPartyId BIGINT = NULL
	DECLARE @Operator VARCHAR(1000) = NULL

 IF @OperatorCode IS NOT NULL AND @OperatorValue IS NOT NULL
 BEGIN
	SET @Operator = @OperatorCode + '|' + @OperatorValue
 END
  
 BEGIN TRY      
          
  UPDATE Enterprise.ExternalUserRelationship      
   SET       
    ThirdPartyRelationshipId = @ThirdPartyRelationshipId,      
    CompanyName = @CompanyName,      
    ThirdPartyCompanyPartyId = NULL,    
    OperatorValue = @Operator  
  WHERE      
   UserLoginPersonaId = @UserLoginPersonaId      
      
  IF @@ERROR = 0 AND @@ROWCOUNT = 0      
  BEGIN      
   INSERT INTO Enterprise.ExternalUserRelationship ( UserLoginPersonaId, ThirdPartyRelationshipId, CompanyName, ThirdPartyCompanyPartyId, OperatorValue )      
   VALUES      
    ( @UserLoginPersonaId, @ThirdPartyRelationshipId, @CompanyName, @CompanyPartyId, @Operator)      
  END      
      
  SELECT  @UserLoginPersonaId AS Id,      
    '' AS ErrorMessage      
 END TRY      
 BEGIN CATCH      
  DECLARE @ErrorLogID INT;      
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;      
      
        SELECT  0 AS Id,      
    ErrorMessage      
        FROM    dbo.ErrorLog      
        WHERE   ErrorLogID = @ErrorLogID;      
 END CATCH      
END
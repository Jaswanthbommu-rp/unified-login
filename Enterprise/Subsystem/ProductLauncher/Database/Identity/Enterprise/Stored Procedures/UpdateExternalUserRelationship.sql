CREATE PROCEDURE [Enterprise].[UpdateExternalUserRelationship]
(
	@UserLoginPersonaId BIGINT,
	@ThirdPartyRelationshipId TINYINT,
	@CompanyName VARCHAR(200) = NULL,
	@ThirdPartyCompanyRealPageId UNIQUEIDENTIFIER = NULL
)
AS
BEGIN
	DECLARE @CompanyPartyId BIGINT = NULL
	IF (@ThirdPartyCompanyRealPageId IS NOT NULL )
	BEGIN
    	SELECT @CompanyPartyId = PartyId FROM Enterprise.Party WHERE RealPageId = @ThirdPartyCompanyRealPageId
	END
	BEGIN TRY
    
		UPDATE Enterprise.ExternalUserRelationship
			SET 
				ThirdPartyRelationshipId = @ThirdPartyRelationshipId,
				CompanyName = @CompanyName,
				ThirdPartyCompanyPartyId = @CompanyPartyId
		WHERE
			UserLoginPersonaId = @UserLoginPersonaId

		IF @@ERROR = 0 AND @@ROWCOUNT = 0
		BEGIN
			INSERT INTO Enterprise.ExternalUserRelationship ( UserLoginPersonaId, ThirdPartyRelationshipId, CompanyName, ThirdPartyCompanyPartyId )
			VALUES
				( @UserLoginPersonaId, @ThirdPartyRelationshipId, @CompanyName, @CompanyPartyId )
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


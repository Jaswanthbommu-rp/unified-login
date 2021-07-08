CREATE PROCEDURE [Logging].[DeleteCompanyActivityData] (
	@OrganizationPartyId INT
)
AS
BEGIN
	BEGIN TRY

		DELETE AD
		FROM logging.ActivityDetail AD INNER JOIN logging.Activity A ON A.ActivityId = AD.ActivityId
		WHERE
			A.OrganizationPartyId = @OrganizationPartyId

		DELETE FROM logging.Activity WHERE OrganizationPartyId = @OrganizationPartyId 

		DELETE FROM	
			Logging.UserLogin 
		WHERE 
			UserId NOT IN
			( SELECT CreatedBy AS FromUserId FROM logging.Activity UNION SELECT ContextReferenceId AS ToUserId FROM logging.Activity )

	END TRY
	BEGIN CATCH

		SELECT 0 AS Id,ERROR_MESSAGE() AS ErrorMessage
		RETURN 0;

	END CATCH;
	
	SELECT 1 AS Id, '' AS ErrorMessage;
END

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
			( SELECT FromUserId FROM logging.Activity UNION SELECT ToUserId FROM logging.Activity )
	END TRY
	BEGIN CATCH
		DECLARE @ErrorLogID int;
		EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;
		SELECT 0 AS Id, ErrorMessage
		FROM dbo.ErrorLog
		WHERE ErrorLogID = @ErrorLogID;
		RETURN 0;
	END CATCH;
	
	SELECT 1 AS Id, '' AS ErrorMessage;
END

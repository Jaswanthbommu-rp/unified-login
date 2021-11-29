CREATE PROCEDURE [Security].[GetOrganizationOverRideRights]
AS
    BEGIN
		SELECT 
		RightId,
		OrgPartyId,
		VisibilityStatusId,
		CreatedBy,
		CreatedDate
		FROM [Security].[OrganizationOverRideRight] 
    END;

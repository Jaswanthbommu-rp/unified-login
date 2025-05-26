CREATE PROCEDURE [Enterprise].[GetOrganizationTypeADGroups]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        ot.OrganizationTypeId,
        ot.Name AS OrganizationTypeName,
        ag.ADGroupId,
        adg.DisplayName AS ADGroupName
    FROM 
        Enterprise.OrganizationType ot
    LEFT JOIN 
        Security.ADGroupOrganizationType ag
        ON ot.OrganizationTypeId = ag.OrganizationTypeId
    LEFT JOIN 
        Security.ADGroup adg
        ON ag.ADGroupId = adg.ADGroupId
    WHERE 
        ot.ThruDate IS NULL
    ORDER BY 
        ot.Name,
        ag.ADGroupId;
END

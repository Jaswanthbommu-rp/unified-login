CREATE PROCEDURE [Enterprise].[GetAllStatusTypes]
AS
	SELECT StatusTypeId, [Name]
	FROM Enterprise.StatusType;

CREATE PROCEDURE [Enterprise].[GetAllStatusTypes]
AS
	SELECT StatusTypeId As Id, [Name]
	FROM Enterprise.StatusType;

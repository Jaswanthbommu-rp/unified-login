CREATE PROCEDURE [Enterprise].[GetPropertyInstanceListById]
	@InstanceList [Enterprise].[PropertyInstanceType] READONLY
AS
BEGIN
	SET NOCOUNT ON
	SELECT
		PI1.[PropertyInstanceId]
		,[Name]
		,[Address]
		,[City]
		,[State]
		,[PostalCode]
		,[Country]
		,[County]
		,[Latitude]
		,[Longitude]
		,PI1.[InstanceId]
		,PI1.CustomerPropertyId
		,PI1.Domain

	FROM 
		[Enterprise].[PropertyInstance] pi1
		INNER JOIN @InstanceList IL
			ON IL.InstanceId = PI1.InstanceId
	ORDER BY Name
END

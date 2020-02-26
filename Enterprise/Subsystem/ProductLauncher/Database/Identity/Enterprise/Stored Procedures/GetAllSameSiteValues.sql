
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the Comparator table.
-- =============================================
CREATE PROCEDURE [Enterprise].[GetAllSameSiteValues] 

 AS 

	SELECT 
		 LogicalGroup.LogicalGrouper
		,LogicalGroup.Sequence
		,ComparatorLeft.Name AS 'ComparatorLeft'
		,SameSiteValueLeft.SameSiteName AS 'SameSiteValueLeft'
		,LogicalOperator.Name AS 'LogicalOperator'
		,ComparatorRight.Name AS 'ComperatorRight'
		,SameSiteValueRight.SameSiteName AS 'SameSiteValueRight'
	FROM
		Enterprise.SameSiteValue
	INNER JOIN Enterprise.SameSiteValue AS SameSiteValueLeft ON SameSitevalue.SameSiteValueId = SameSiteValueLeft.SameSiteValueID
	INNER JOIN Enterprise.Comparator AS ComparatorLeft ON ComparatorLeft.ComparatorID = SameSiteValueLeft.ComparatorID
	LEFT OUTER JOIN Enterprise.LogicalGroup ON LogicalGroup.SameSiteIdLeft = SameSiteValueLeft.SameSiteValueId
	LEFT OUTER JOIN Enterprise.LogicalOperator ON LogicalOperator.LogicalOperatorId = LogicalGroup.LogicalOperatorId
	LEFT OUTER JOIN Enterprise.SameSiteValue AS SameSiteValueRight ON LogicalGroup.SameSiteIdRight = SameSiteValueRight.SameSiteValueId
	LEFT OUTER JOIN Enterprise.Comparator AS ComparatorRight ON ComparatorRight.ComparatorID = SameSiteValueRight.ComparatorID
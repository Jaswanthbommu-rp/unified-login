CREATE PROCEDURE [Enterprise].[GetStatusTypes] (
	@StatusTypeCategoryTypeName varchar(50),
	@StatusTypeCategoryName varchar(50)
)
AS
BEGIN
	SELECT	est.StatusTypeId,
				est.Name
	FROM	Enterprise.StatusTypeCategoryType estct
				INNER JOIN Enterprise.StatusTypeCategory estc ON estc.StatusTypeCategoryTypeId = estct.StatusTypeCategoryTypeId
				INNER JOIN Enterprise.StatusTypeCategoryClassification estcc ON estcc.StatusTypeCategoryId = estc.StatusTypeCategoryId
				INNER JOIN Enterprise.StatusType est ON est.StatusTypeId = estcc.StatusTypeId
	WHERE	estct.Name = @StatusTypeCategoryTypeName
	AND		estc.Name = @StatusTypeCategoryName
	ORDER BY est.Name
END
CREATE VIEW [Enterprise].[CreateUserSourceType]
AS
SELECT STCT.StatusTYpeCategoryTypeID As 'CategoryTypeId'
		,STCT.Name 'CategoryType' 
		,STC.StatusTypeCategoryId AS 'CategoryId'
		,STC.Name AS 'CategoryName'
		,ST.StatusTypeId AS 'TypeId'
		,ST.Name AS 'TypeName'
FROM Enterprise.StatusTypeCategoryType STCT
     JOIN Enterprise.StatusTypeCategory STC ON STC.StatusTypeCategoryTypeId = STCT.StatusTypeCategoryTypeId
     JOIN Enterprise.StatusTypeCategoryClassification STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
     JOIN Enterprise.StatusType ST ON ST.StatusTypeId = STCC.StatusTypeId
	 WHERE STCT.Name = 'Unified Platform'
		AND STC.Name = 'Create User Source'
GO

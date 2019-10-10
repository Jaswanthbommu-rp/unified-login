CREATE PROCEDURE [Enterprise].[ListRoleCategory]
AS
     BEGIN
         SELECT STCT.Name 'Class',
                STC.Name 'Category',
                ST.Name 'Type',
			 ST.StatusTypeId 'TypeId'
         FROM Enterprise.StatusTypeCategoryType STCT
              JOIN Enterprise.StatusTypeCategory STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
              JOIN Enterprise.StatusTypeCategoryClassification STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
              JOIN Enterprise.StatusType ST ON ST.StatusTypeId = STCC.StatusTypeId
         WHERE STC.Name = 'Role Type';
     END;
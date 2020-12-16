CREATE PROCEDURE [Security].[ListSecurityStatus]
AS
   BEGIN
        SELECT STCT.Name 'Category' , STC.Name 'CategoryType', ST.Name 'Status', ST.StatusTypeId
		FROM Enterprise.StatusTypeCategoryType STCT
			JOIN Enterprise.StatusTypeCategory  STC
				ON STC.StatusTypeCategoryTypeId = STCT.StatusTypeCategoryTypeId
			JOIN Enterprise.StatusTypeCategoryClassification  STCC
				ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
			JOIN Enterprise.StatusType ST
				ON ST.StatusTypeId = STCC.StatusTypeId
		WHERE STCT.name = 'Security'    
   END;
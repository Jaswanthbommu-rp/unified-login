CREATE PROCEDURE [Enterprise].[GetULPickListByCategory] @categoryName varchar(50)
AS
BEGIN
SELECT *
	FROM [Enterprise].[SettingPicklist]
	WHERE CategoryName = @categoryName
	AND ThruDate is NULL
END